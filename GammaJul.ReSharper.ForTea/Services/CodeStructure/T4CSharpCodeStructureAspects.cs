﻿#region License
//    Copyright 2012 Julien Lebosquain
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
#endregion


using System.Collections.Generic;
using JetBrains.Application.UI.Controls.TreeView;
using JetBrains.Application.UI.Controls.Utils;
using JetBrains.Application.UI.TreeModels;
using JetBrains.DocumentModel;
using JetBrains.ReSharper.Feature.Services.CodeStructure;
using JetBrains.ReSharper.Feature.Services.Resources;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.Util;

namespace GammaJul.ReSharper.ForTea.Services.CodeStructure {

	/// <summary>
	/// We can't inherit from CSharpCodeStructureAspect since it's internal, so we have to duplicate a bit of functionality from R# here.
	/// </summary>
	internal sealed class T4CSharpCodeStructureAspects : CodeStructureDeclarationAspects {

		private readonly T4CSharpCodeStructureDeclaredElement _element;

		public override bool InitiallyExpanded {
			get { return _element.InitiallyExpanded; }
		}

		protected override IList<string> CalculateQuickSearchTexts(IDeclaration declaration) {
			if (!declaration.IsValid())
				return EmptyList<string>.InstanceList;

			var owner = declaration as IInterfaceQualificationOwner;
			if (owner != null && owner.InterfaceQualificationReference != null)
				return new[] { owner.GetDeclaredShortName(), owner.InterfaceQualificationReference.ShortName };

			var constructorDeclaration = declaration as IConstructorDeclaration;
			if (constructorDeclaration != null)
				return new[] { constructorDeclaration.DeclaredName, "new", "ctor" };

			var indexerDeclaration = declaration as IIndexerDeclaration;
			if (indexerDeclaration != null)
				return new[] { indexerDeclaration.DeclaredName, "this" };

			var destructorDeclaration = declaration as IDestructorDeclaration;
			if (destructorDeclaration != null)
				return new[] { destructorDeclaration.DeclaredName, "Finalize" };

			var operatorDeclaration = declaration as IOperatorDeclaration;
			if (operatorDeclaration != null)
				return new[] { operatorDeclaration.DeclaredName, "operator" };

			var eventDeclaration = declaration as IEventDeclaration;
			if (eventDeclaration != null)
				return new[] { eventDeclaration.DeclaredName, "event" };

			return base.CalculateQuickSearchTexts(declaration);
		}

		public override void Present(StructuredPresenter<TreeModelNode, IPresentableItem> presenter, IPresentableItem item,
			TreeModelNode modelNode, PresentationState state) {
			base.Present(presenter, item, modelNode, state);
			if (_element.InheritanceInformation != null)
				item.Images.Add(_element.InheritanceInformation.Image, _element.InheritanceInformation.ToolTip);
			else {
				// if the children have inheritance information, we must add en empty inheritance icon so that the text is aligned
				var structureDeclaredElement = _element.Parent as T4CSharpCodeStructureDeclaredElement;
				if (structureDeclaredElement != null && structureDeclaredElement.ChildrenWithInheritance)
					item.Images.Add(PsiServicesThemedIcons.Empty.Id);
			}
		}
		
		public override DocumentRange[] GetNavigationRanges() {
			if (!Declaration.IsValid())
				return EmptyArray<DocumentRange>.Instance;

			if (Declaration is IClassLikeDeclaration || Declaration is INamespaceDeclaration) {
				return new[] {
					Declaration.GetNavigationRange(),
					Declaration.GetLastTokenIn().GetNavigationRange()
				};
			}

			return base.GetNavigationRanges();
		}

		public T4CSharpCodeStructureAspects(T4CSharpCodeStructureDeclaredElement element, IDeclaration declaration)
			: base(declaration) {
			_element = element;
		}

	}

}