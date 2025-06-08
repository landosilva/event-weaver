using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using MethodAttributes = Mono.Cecil.MethodAttributes;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace Lando.EventWeaver.Editor
{
    [InitializeOnLoad]
    public static class EventWeaver
    {
        static EventWeaver()
        {
            CompilationPipeline.assemblyCompilationFinished += OnAssemblyCompiled;
        }

        private static void OnAssemblyCompiled(string compiledAssemblyPath, CompilerMessage[] compilerMessages)
        {
            try
            {
                string fileName = Path.GetFileName(compiledAssemblyPath);

                using (ModuleDefinition module = ModuleDefinition.ReadModule(compiledAssemblyPath))
                {
                    bool referencesEventWeaver = module.AssemblyReferences.Any(HasAssemblyPrefix);

                    if (!referencesEventWeaver)
                        return;
                }

                Debug.Log($"{InformationMessage.PatchingAssembly}{fileName}");
                PatchAssembly(compiledAssemblyPath);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"{WarningMessage.FailedToPatchAssembly}{compiledAssemblyPath}': {ex.Message}");
            }
        }

        private static void PatchAssembly(string assemblyPath)
        {
            
            DefaultAssemblyResolver resolver = new();
            string asmDir = Path.GetFullPath(Path.Combine(Application.dataPath, FolderName.ScriptAssemblies));
            resolver.AddSearchDirectory(asmDir);

            string managedPath = Path.Combine(EditorApplication.applicationContentsPath, FolderName.Managed);
            resolver.AddSearchDirectory(managedPath);
            string enginePath = Path.Combine(managedPath, FolderName.UnityEngine);
            if (Directory.Exists(enginePath))
                resolver.AddSearchDirectory(enginePath);

            PackageInfo packageInfo = PackageInfo.FindForAssembly(typeof(EventRegistry).Assembly);
            if (packageInfo != null)
            {
                string packagePath = Path.Combine(packageInfo.resolvedPath, FolderName.Runtime);
                resolver.AddSearchDirectory(packagePath);
            }

            ReaderParameters readerParams = new() { ReadWrite = true, AssemblyResolver = resolver };
            ModuleDefinition module = ModuleDefinition.ReadModule(assemblyPath, readerParams);

            TypeDefinition eventRegistryType = ResolveEventRegistryType(module);
            if (eventRegistryType == null)
            {
                Debug.LogWarning(WarningMessage.EventRegistryNotFound);
                module.Dispose();
                return;
            }

            MethodDefinition registerDefinition = eventRegistryType.Methods.FirstOrDefault(methodDefinition => methodDefinition.Name == MethodName.Register && methodDefinition.HasGenericParameters && methodDefinition.Parameters.Count == 1);
            MethodDefinition unregisterDefinition = eventRegistryType.Methods.FirstOrDefault(methodDefinition => methodDefinition.Name == MethodName.Unregister && methodDefinition.HasGenericParameters && methodDefinition.Parameters.Count == 1);
            if (registerDefinition == null || unregisterDefinition == null)
            {
                Debug.LogWarning(WarningMessage.RegisterUnregisterNotFound);
                module.Dispose();
                return;
            }

            foreach (TypeDefinition type in module.Types.Where(methodDefinition => !methodDefinition.IsAbstract))
            {
                List<GenericInstanceType> listeners = GetAllEventListenerInterfaces(type);
                if (!listeners.Any())
                    continue;

                bool isMono = InheritsFrom(type, ClassName.MonoBehaviour);
                if (isMono)
                    InjectMonoBehaviour(type, listeners, registerDefinition, unregisterDefinition, module);
                else
                    InjectPlainClass(type, listeners, registerDefinition, unregisterDefinition, module);
            }

            module.Write();
            module.Dispose();
        }
    
        private static List<GenericInstanceType> GetAllEventListenerInterfaces(TypeDefinition type)
        {
            var result = new List<GenericInstanceType>();
            TypeDefinition current = type;
            while (current != null)
            {
                IEnumerable<GenericInstanceType> listeners = GetEventListenerInterfaces(current);
                result.AddRange(listeners);
                try { current = current.BaseType?.Resolve(); }
                catch { break; }
            }
            return result;
        }
        
        private static IEnumerable<GenericInstanceType> GetEventListenerInterfaces(TypeDefinition type)
        {
            return type.Interfaces.Select(TypeReference).OfType<GenericInstanceType>().Where(IsEventListener);

            bool IsEventListener(GenericInstanceType genericInstanceType) 
                => genericInstanceType.ElementType.Name == ClassName.EventListener;
            TypeReference TypeReference(InterfaceImplementation interfaceImplementation) 
                => interfaceImplementation.InterfaceType;
        }

        private static TypeDefinition ResolveEventRegistryType(ModuleDefinition module)
        {
            TypeDefinition found = module.Types.FirstOrDefault(HasRegistryName);
            if (found != null) 
                return found;

            IEnumerable<AssemblyNameReference> assemblyReferences = module.AssemblyReferences.Where(HasAssemblyPrefix);
            foreach (AssemblyNameReference assemblyNameReference in assemblyReferences)
            {
                AssemblyDefinition assemblyDefinition;
                try { assemblyDefinition = module.AssemblyResolver.Resolve(assemblyNameReference); }
                catch { continue; }

                found = assemblyDefinition.MainModule.Types.FirstOrDefault(HasRegistryName);
                if (found != null) 
                    return found;
            }
            
            return null;
        }

        private static bool InheritsFrom(TypeDefinition type, string baseName)
        {
            TypeDefinition current = type;
            while (current.BaseType != null && current.BaseType.FullName != ClassName.SystemObject)
            {
                if (current.BaseType.Name == baseName)
                    return true;

                try
                {
                    current = current.BaseType.Resolve();
                }
                catch
                {
                    break;
                }
            }
            return false;
        }

        private static void InjectMonoBehaviour(
            TypeDefinition type,
            List<GenericInstanceType> listeners,
            MethodDefinition registerDefinition,
            MethodDefinition unregisterDefinition,
            ModuleDefinition module)
        {
            MethodDefinition onEnableMethod  = GetOrCreateOverride(type, MethodName.OnEnable,  module.TypeSystem.Void);
            MethodDefinition onDisableMethod = GetOrCreateOverride(type, MethodName.OnDisable, module.TypeSystem.Void);

            InsertCalls(onEnableMethod,  listeners, registerDefinition,   module);
            InsertCalls(onDisableMethod, listeners, unregisterDefinition, module);
        }

        private static void InjectPlainClass(
            TypeDefinition type,
            List<GenericInstanceType> listeners,
            MethodDefinition registerDefinition,
            MethodDefinition unregisterDefinition,
            ModuleDefinition module)
        {
            MethodDefinition constructorMethodDefinition = type.Methods.FirstOrDefault(methodDefinition => methodDefinition.IsConstructor && !methodDefinition.IsStatic)
                                    ?? CreateConstructor(type, module);
            InsertCalls(constructorMethodDefinition, listeners, registerDefinition, module);

            MethodDefinition finalizerMethodDefinition = type.Methods.FirstOrDefault(methodDefinition => methodDefinition.Name == MethodName.Finalize && methodDefinition.IsVirtual)
                                                         ?? CreateFinalizer(type, module);
            InsertCalls(finalizerMethodDefinition, listeners, unregisterDefinition, module);
        }

        private static MethodDefinition GetOrCreateOverride(
            TypeDefinition type,
            string name,
            TypeReference returnType)
        {
            MethodDefinition existing = type.Methods.FirstOrDefault(methodDefinition => methodDefinition.Name == name && !methodDefinition.HasParameters);
            if (existing != null) 
                return existing;

            MethodDefinition methodDefinition = new(
                name,
                attributes: MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                returnType);

            ILProcessor il = methodDefinition.Body.GetILProcessor();
            il.Append(instruction: il.Create(opcode: OpCodes.Ret));
            type.Methods.Add(methodDefinition);
            return methodDefinition;
        }

        private static void InsertCalls(
            MethodDefinition method,
            List<GenericInstanceType> listeners,
            MethodDefinition definition,
            ModuleDefinition module)
        {
            ILProcessor il = method.Body.GetILProcessor();
            Instruction target = method.Body.Instructions.First(instruction => instruction.OpCode != OpCodes.Nop);

            foreach (GenericInstanceType gi in listeners)
            {
                GenericInstanceMethod genericInstanceMethod = new(definition);
                genericInstanceMethod.GenericArguments.Add(module.ImportReference(gi.GenericArguments[0]));
                MethodReference methodReference = module.ImportReference(genericInstanceMethod);

                il.InsertBefore(target, instruction: il.Create(opcode: OpCodes.Ldarg_0));
                il.InsertBefore(target, instruction: il.Create(opcode: OpCodes.Call, methodReference));
            }
        }

        private static MethodDefinition CreateConstructor(
            TypeDefinition type,
            ModuleDefinition module)
        {
            MethodDefinition constructor = new(
                MethodName.Constructor,
                attributes: MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                module.TypeSystem.Void);

            ILProcessor il = constructor.Body.GetILProcessor();
            il.Append(instruction: il.Create(opcode: OpCodes.Ldarg_0));
            
            if (type.BaseType != null && type.BaseType.FullName != ClassName.SystemObject)
            {
                TypeDefinition baseTypeDefinition = type.BaseType.Resolve();
                MethodDefinition baseConstructor = baseTypeDefinition.Methods
                    .FirstOrDefault(methodDefinition => methodDefinition.IsConstructor && !methodDefinition.IsStatic && methodDefinition.Parameters.Count == 0);

                if (baseConstructor != null) 
                    il.Append(instruction: il.Create(opcode: OpCodes.Call, method: module.ImportReference(baseConstructor)));
            }

            il.Append(instruction: il.Create(OpCodes.Ret));
            type.Methods.Add(constructor);
            return constructor;
        }

        private static MethodDefinition CreateFinalizer(
            TypeDefinition type,
            ModuleDefinition module)
        {
            MethodDefinition finalizer = new(
                MethodName.Finalize,
                attributes: MethodAttributes.Family | MethodAttributes.Virtual | MethodAttributes.HideBySig,
                module.TypeSystem.Void);

            ILProcessor il = finalizer.Body.GetILProcessor();

            if (type.BaseType != null && type.BaseType.FullName != ClassName.SystemObject)
            {
                try
                {
                    il.Append(instruction: il.Create(opcode: OpCodes.Ldarg_0));

                    MethodReference baseFinalize = module.ImportReference(
                        method: type.BaseType.Resolve().Methods.FirstOrDefault(
                            methodDefinition => methodDefinition.Name == MethodName.Finalize && methodDefinition.Parameters.Count == 0));

                    if (baseFinalize != null)
                    {
                        il.Append(instruction: il.Create(opcode: OpCodes.Callvirt, baseFinalize));
                    }
                }
                catch
                {
                    // Skip base.Finalize if type.BaseType can't be resolved
                }
            }

            il.Append(instruction: il.Create(opcode: OpCodes.Ret));
            type.Methods.Add(finalizer);
            return finalizer;
        }
        
        private static bool HasRegistryName(TypeDefinition typeDefinition) 
            => typeDefinition.Name == ClassName.EventRegistry;
        private static bool HasAssemblyPrefix(AssemblyNameReference assemblyNameReference) 
            => assemblyNameReference.Name.StartsWith(AssemblyName.EventWeaver);
    }
}