// forin.cs
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
    public sealed class ForEachStatement : AstNode
    {
        private AstNode m_variable;
        public AstNode Variable 
        {
            get { return m_variable; }
            set
            {
                if (value != m_variable)
                {
                    if (m_variable != null && m_variable.Parent == this)
                    {
                        m_variable.Parent = null;
                    }
                    m_variable = value;
                    if (value != null)
                    {
                        value.Parent = this;
                    }
                }
            }
        }

        private AstNode m_collection;
        public AstNode Collection
        {
            get { return m_collection; }
            set
            {
                if (value != m_collection)
                {
                    if (m_collection != null && m_collection.Parent == this)
                    {
                        m_collection.Parent = null;
                    }
                    m_collection = value;
                    if (value != null)
                    {
                        value.Parent = this;
                    }
                }
            }
        }

        private Block m_body;
        public Block Body
        {
            get { return m_body; }
            set
            {
                if (value != m_body)
                {
                    if (m_body != null && m_body.Parent == this)
                    {
                        m_body.Parent = null;
                    }
                    m_body = value;
                    if (value != null)
                    {
                        value.Parent = this;
                    }
                }
            }
        }

        public ForEachStatement(Context context, JSParser parser, AstNode var, AstNode collection, AstNode body)
            : base(context, parser)
        {
            Variable = var;
            Collection = collection;
            Body = ForceToBlock(body);
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
                return EnumerateNonNullNodes(Variable, Collection, Body);
            }
        }

        public override bool ReplaceChild(AstNode oldNode, AstNode newNode)
        {
            if (Variable == oldNode)
            {
                Variable = newNode;
                return true;
            }
            if (Collection == oldNode)
            {
                Collection = newNode;
                return true;
            }
            if (Body == oldNode)
            {
                Body = ForceToBlock(newNode);
                return true;
            }
            return false;
        }

        public override bool RequiresSeparator
        {
            get
            {
                // requires a separator if the body does
                return Body == null ? true : Body.RequiresSeparator;
            }
        }

        internal override bool EndsWithEmptyBlock
        {
            get
            {
                return Body == null ? true : Body.EndsWithEmptyBlock;
            }
        }

        //public override string ToCode(ToCodeFormat format)
        //{
        //    StringBuilder sb = new StringBuilder();
        //    sb.Append("for(");

        //    string var = Variable.ToCode();
        //    sb.Append(var);
        //    if (JSScanner.EndsWithIdentifierPart(var))
        //    {
        //        sb.Append(' ');
        //    }
        //    sb.Append("in");

        //    string collection = Collection.ToCode();
        //    if (JSScanner.StartsWithIdentifierPart(collection))
        //    {
        //        sb.Append(' ');
        //    }
        //    sb.Append(Collection.ToCode());
        //    sb.Append(')');

        //    string bodyString = (
        //      Body == null
        //      ? string.Empty
        //      : Body.ToCode()
        //      );
        //    sb.Append(bodyString);
        //    return sb.ToString();
        //}
    }
}
