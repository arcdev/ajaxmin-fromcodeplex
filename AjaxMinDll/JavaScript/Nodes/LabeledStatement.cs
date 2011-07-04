// labeledstatement.cs
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
    public sealed class LabeledStatement : AstNode
    {
        private AstNode m_statement;
        public AstNode Statement
        {
            get { return m_statement; }
            set
            {
                if (value != m_statement)
                {
                    if (m_statement != null && m_statement.Parent == this)
                    {
                        m_statement.Parent = null;
                    }
                    m_statement = value;
                    if (value != null)
                    {
                        value.Parent = this;
                    }
                }
            }
        }

        public string Label { get; set; }
        public string AlternateLabel { get; set; }
        public int NestLevel { get; set; }

        public LabeledStatement(Context context, JSParser parser, string label, AstNode statement)
            : base(context, parser)
        {
            Label = label;
            Statement = statement;
        }

        public override void Accept(IVisitor visitor)
        {
            if (visitor != null)
            {
                visitor.Visit(this);
            }
        }

        public override bool RequiresSeparator
        {
            get
            {
                // requires a separator if the statement does
                return (m_statement != null ? m_statement.RequiresSeparator : false);
            }
        }

        internal override bool EndsWithEmptyBlock
        {
            get
            {
                return (m_statement != null ? m_statement.EndsWithEmptyBlock : false);
            }
        }

        public override AstNode LeftHandSide
        {
            get
            {
                // the label is on the left, but it's sorta ignored
                return (m_statement != null ? m_statement.LeftHandSide : null);
            }
        }

        public override IEnumerable<AstNode> Children
        {
            get
            {
                return EnumerateNonNullNodes(m_statement);
            }
        }

        public override bool ReplaceChild(AstNode oldNode, AstNode newNode)
        {
            if (Statement == oldNode)
            {
                Statement = newNode;
                return true;
            }
            return false;
        }

        //public override string ToCode(ToCodeFormat format)
        //{
        //    StringBuilder sb = new StringBuilder();
        //    if (NestLevel >= 0
        //        && Parser.Settings.LocalRenaming != LocalRenaming.KeepAll
        //        && Parser.Settings.IsModificationAllowed(TreeModifications.LocalRenaming))
        //    {
        //        // we're hyper-crunching.
        //        // we want to output our label as per our nested level.
        //        // top-level is "a", next level is "b", etc.
        //        // we don't need to worry about collisions with variables.
        //        sb.Append(BindingMinifier.CrunchedLabel(NestLevel));
        //    }
        //    else
        //    {
        //        // not hypercrunching -- just output our label
        //        sb.Append(Label);
        //    }
        //    sb.Append(':');
        //    if (m_statement != null)
        //    {
        //        // don't sent the AlwaysBraces down the chain -- we're handling it here.
        //        // but send any other formats down -- we don't know why they were sent.
        //        sb.Append(m_statement.ToCode(format));
        //    }
        //    return sb.ToString();
        //}

    }
}
