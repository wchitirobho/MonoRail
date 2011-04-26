﻿//  Copyright 2004-2011 Castle Project - http://www.castleproject.org/
//  Hamilton Verissimo de Oliveira and individual contributors as indicated. 
//  See the committers.txt/contributors.txt in the distribution for a 
//  full listing of individual contributors.
// 
//  This is free software; you can redistribute it and/or modify it
//  under the terms of the GNU Lesser General Public License as
//  published by the Free Software Foundation; either version 3 of
//  the License, or (at your option) any later version.
// 
//  You should have received a copy of the GNU Lesser General Public
//  License along with this software; if not, write to the Free
//  Software Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA
//  02110-1301 USA, or see the FSF site: http://www.fsf.org.

namespace Castle.MonoRail.Hosting.Mvc.Typed

    open System
    open System.Collections
    open System.Collections.Generic
    open System.Linq
    open System.Reflection
    open System.Web
    open System.ComponentModel.Composition
    open Castle.MonoRail.Routing
    open Castle.MonoRail.Framework
    open Castle.MonoRail.Hosting.Mvc
    open Castle.MonoRail.Hosting.Mvc.Extensibility

    [<Interface>]
    type IAspNetHostingBridge = 
        abstract member ReferencedAssemblies : IEnumerable<Assembly>


    [<Export(typeof<IAspNetHostingBridge>)>]
    type BuildManagerAdapter() = 
        interface IAspNetHostingBridge with 
            member x.ReferencedAssemblies 
                with get() = 
                    let assemblies = System.Web.Compilation.BuildManager.GetReferencedAssemblies()
                    assemblies.Cast<Assembly>()

    [<AbstractClass>]
    type BaseTypeBasedControllerProvider() = 
        inherit ControllerProvider()
        let mutable _desc_builder = Unchecked.defaultof<ControllerDescriptorBuilder>

        [<Import>]
        member this.ControllerDescriptorBuilder
            with get() = _desc_builder and set(v) = _desc_builder <- v

        abstract ResolveControllerType : data:RouteMatch * context:HttpContextBase -> System.Type
        abstract ActivateController : cType:System.Type * desc:ControllerDescriptor -> obj
        abstract BuildPrototype : inst:obj * desc:ControllerDescriptor -> ControllerPrototype

        default this.BuildPrototype(inst:obj, desc:ControllerDescriptor) = 
            TypedControllerPrototype(desc, inst) :> ControllerPrototype
            
        default this.ActivateController(cType:Type, desc:ControllerDescriptor) = 
            Activator.CreateInstance(cType) 

        override this.Create(data:RouteMatch, context:HttpContextBase) = 
            let cType = this.ResolveControllerType(data, context)
            if (cType <> null) then
                let desc = _desc_builder.Build(cType)
                let instance = this.ActivateController(cType, desc)
                this.BuildPrototype(instance, desc)
            else
                Unchecked.defaultof<_>
    
    and
        [<ControllerProviderExport(8000000)>]
        MefControllerProvider() =
            inherit BaseTypeBasedControllerProvider()

            override this.ResolveControllerType(data:RouteMatch, context:HttpContextBase) = 
                Unchecked.defaultof<_>

    and
        [<ControllerProviderExport(9000000)>] 
        ReflectionBasedControllerProvider [<ImportingConstructor>] (hosting:IAspNetHostingBridge) =
            inherit BaseTypeBasedControllerProvider()
            let _hosting = hosting
            let _entries = Dictionary<string,Type>(StringComparer.OrdinalIgnoreCase)
        
            do
                let size_of_controller = "Controller".Length
            
                seq { 
                        for asm in _hosting.ReferencedAssemblies do 
                            let all_types = 
                                Helpers.typesInAssembly asm (fun t -> not t.IsAbstract && t.Name.EndsWith("Controller"))
                            yield all_types
                    }
                |> Seq.concat
                |> Seq.iter (fun t -> 
                    let name = t.Name.Substring(0, t.Name.Length - size_of_controller)
                    _entries.[name] <- t )

            override this.ResolveControllerType(data:RouteMatch, context:HttpContextBase) = 
                let name = data.RouteParams.["controller"]
                if (name <> null) then
                    let r, typ = _entries.TryGetValue name
                    if (r) then
                        typ
                    else
                        null
                else
                    null

    and 
        TypedControllerPrototype(desc, instance) = 
            inherit ControllerPrototype(instance)
            let _desc = desc
            member t.Descriptor 
                with get() = _desc






    





