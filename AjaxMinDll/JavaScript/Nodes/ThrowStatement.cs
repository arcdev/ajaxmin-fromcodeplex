// throw.cs
//
// Copyright 2010 Microsoft Corporation
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Collections.Generic;
using System.Text;

using Microsoft.Ajax.Utilities.JavaScript;
using Microsoft.Ajax.Utilities.JavaScript.Visitors;

namespace Microsoft.Ajax.Utilities.JavaScript.Nodes
{
    public sealed class ThrowStatement : AstNode
    {
        private AstNode m_operand;
        public AstNode Operand
        {
            get { return m_operand; }
            set
            {
                if (value != m_operand)
                {
                    if (m_operand != null && m_operand.Parent == this)
                    {
                        m_operand.Parent = null;
                    }
                    m_operand = value;
                    if (value != null)
                    {
                        value.Parent = this;
                    }
                }
            }
        }

        public ThrowStatement(Context context, JSParser parser, AstNode operand)
            : base(context, parser)
        {
            Operand = operand;
        }

        public override void Accept(IVisitor visitor)
        {
            if (visitor != null)
            {
                visitor.Visit(this);
            }
        }

        public override IEnumerable<AstNode> Children
        {
            get
            {
                return EnumerateNonNullNodes(Operand);
            }
        }

        public override bool ReplaceChild(AstNode oldNode, AstNode newNode)
        {
            if (Operand == oldNode)
            {
                Operand = newNode;
                return true;
            }
            return false;
        }

        public override bool RequiresSeparator
        {
            get
            {
                // if MacSafariQuirks is true, then we will be adding the semicolon
                // ourselves every single time and won't need outside code to add it.
                // otherwise we won't be adding it, but it will need it if there's something
                // to separate it from.
                return false;
            }
        }

        //public override string ToCode(ToCodeFormat format)
        //{
        //    StringBuilder sb = new StringBuilder();
        //    sb.Append("throw");
        //    string exprString = (
        //      Operand == null
        //      ? string.Empty
        //      : Operand.ToCode()
        //      );
        //    if (exprString.Length > 0)
        //    {
        //        if (JSScanner.StartsWithIdentifierPart(exprString))
        //        {
        //            sb.Append(' ');
        //        }
        //        sb.Append(exprString);
        //    }
        //    if (Parser.Settings.MacSafariQuirks)
        //    {
        //        sb.Append(';');
        //    }
        //    return sb.ToString();
        //}
    }
}