//***************************************************************************
//
//    Copyright (c) Microsoft Corporation. All rights reserved.
//    This code is licensed under the Visual Studio SDK license terms.
//    THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
//    ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
//    IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
//    PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//***************************************************************************

using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace TodoClassification
{
    /// <summary>
    /// Set the display values for the classification
    /// </summary>
    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "todo")]
    [Name("ToDoText")]
    [UserVisible(true)]
    [Order(After = Priority.High)]
    internal sealed class ToDoFormat : ClassificationFormatDefinition
    {
        public ToDoFormat()
        {
            DisplayName = "ToDo Text"; //human readable version of the name
            BackgroundOpacity = 0;
            ForegroundColor = Colors.CadetBlue;
            //BackgroundColor = Colors.Orange;
        }
    }
}
