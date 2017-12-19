﻿//***************************************************************************
//
//    Copyright (c) Microsoft Corporation. All rights reserved.
//    This code is licensed under the Visual Studio SDK license terms.
//    THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
//    ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
//    IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
//    PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//***************************************************************************

using System.Windows;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using ToDoGlyphFactory;

namespace TodoClassification
{
    /// <summary>
    /// This class implements IGlyphFactory, which provides the visual
    /// element that will appear in the glyph margin.
    /// </summary>
    internal class ToDoGlyphFactory : IGlyphFactory
    {

        public UIElement GenerateGlyph(IWpfTextViewLine line, IGlyphTag tag)
        {
            return new TodoGlyph();
        }
    }
}
