﻿// cccomment.cs
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

using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Ajax.Utilities.JavaScript;
using Microsoft.Ajax.Utilities.JavaScript.Visitors;

namespace Microsoft.Ajax.Utilities.JavaScript.Nodes
{
    public class ConditionalCompilationComment : AstNode
    {
        private Block m_statements;
        public Block Statements {
            get { return m_statements; }
            set
            {
                if (value != m_statements)
                {
                    if (m_statements != null && m_statements.Parent == this)
                    {
                        m_statements.Parent = null;
                    }
                    m_statements = value;
                    if (value != null)
                    {
                        value.Parent = this;
                    }
                }
            }
        }

        public ConditionalCompilationComment(Context context, JSParser parser)
            : base(context, parser)
        {
            Statements = new Block(null, parser);
        }


        public override bool RequiresSeparator
        {
            get
            {
                return Statements.Count > 0 ? Statements[Statements.Count - 1].RequiresSeparator : true;
            }
        }

        public override void Accept(IVisitor visitor)
        {
            if (visitor != null)
            {
                visitor.Visit(this);
            }
        }

        public void Append(AstNode statement)
        {
            if (statement != null)
            {
                Context.UpdateWith(statement.Context);
                Statements.Append(statement);
            }
        }

        public override IEnumerable<AstNode> Children
        {
            get
            {
                return EnumerateNonNullNodes(Statements);
            }
        }

        public override bool ReplaceChild(AstNode oldNode, AstNode newNode)
        {
            if (Statements == oldNode)
            {
                Statements = ForceToBlock(newNode);
                return true;
            }
            return false;
        }

        //public override string ToCode(ToCodeFormat format)
        //{
        //    StringBuilder sb = new StringBuilder();

        //    // if there aren't any statements, we don't need this comment
        //    if (Statements.Count > 0)
        //    {
        //        sb.Append("/*");

        //        // a first conditional compilation statement will take care of the opening @-sign,
        //        // so if the first statement is NOT a conditional compilation statement, then we
        //        // need to take care of it outselves
        //        if (!(Statements[0] is ConditionalCompilationStatement))
        //        {
        //            sb.Append("@ ");
        //        }

        //        sb.Append(Statements.ToCode(ToCodeFormat.NoBraces));
        //        sb.Append("@*/");
        //    }
        //    return sb.ToString();
        //}
    }
}
