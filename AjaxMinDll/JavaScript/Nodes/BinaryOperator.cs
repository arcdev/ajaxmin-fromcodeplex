// binaryop.cs
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
using System.Globalization;
using System.Text;

using Microsoft.Ajax.Utilities.JavaScript;
using Microsoft.Ajax.Utilities.JavaScript.Visitors;

namespace Microsoft.Ajax.Utilities.JavaScript.Nodes
{

    public sealed class BinaryOperator : Expression
    {
        private AstNode m_operand1;
        public AstNode Operand1 {
            get { return m_operand1; }
            set
            {
                if (m_operand1 != value)
                {
                    if (m_operand1 != null && m_operand1.Parent == this)
                    {
                        m_operand1.Parent = null;
                    }
                    m_operand1 = value;
                    if (value != null)
                    {
                        value.Parent = this;
                    }
                }
            }
        }

        private AstNode m_operand2;
        public AstNode Operand2 
        {
            get { return m_operand2; }
            set
            {
                if (value != m_operand2)
                {
                    if (m_operand2 != null && m_operand2.Parent == this)
                    {
                        m_operand2.Parent = null;
                    }
                    m_operand2 = value;
                    if (value != null)
                    {
                        value.Parent = this;
                    }
                }
            }
        }

        public JSToken OperatorToken { get; set; }

        public BinaryOperator(Context context, JSParser parser, AstNode operand1, AstNode operand2, JSToken operatorToken)
            : base(context, parser)
        {
            Operand1 = operand1;
            Operand2 = operand2;
            OperatorToken = operatorToken;
        }

        public override OperatorPrecedence OperatorPrecedence
        {
            get
            {
                return JSScanner.GetOperatorPrecedence(OperatorToken);
            }
        }

        public override PrimitiveType FindPrimitiveType()
        {
            PrimitiveType leftType;
            PrimitiveType rightType;

            switch (OperatorToken)
            {
                case JSToken.Assign:
                case JSToken.Comma:
                    // returns whatever type the right operand is
                    return Operand2.FindPrimitiveType();

                case JSToken.BitwiseAnd:
                case JSToken.BitwiseAndAssign:
                case JSToken.BitwiseOr:
                case JSToken.BitwiseOrAssign:
                case JSToken.BitwiseXor:
                case JSToken.BitwiseXorAssign:
                case JSToken.Divide:
                case JSToken.DivideAssign:
                case JSToken.LeftShift:
                case JSToken.LeftShiftAssign:
                case JSToken.Minus:
                case JSToken.MinusAssign:
                case JSToken.Modulo:
                case JSToken.ModuloAssign:
                case JSToken.Multiply:
                case JSToken.MultiplyAssign:
                case JSToken.RightShift:
                case JSToken.RightShiftAssign:
                case JSToken.UnsignedRightShift:
                case JSToken.UnsignedRightShiftAssign:
                    // always returns a number
                    return PrimitiveType.Number;

                case JSToken.Equal:
                case JSToken.GreaterThan:
                case JSToken.GreaterThanEqual:
                case JSToken.In:
                case JSToken.InstanceOf:
                case JSToken.LessThan:
                case JSToken.LessThanEqual:
                case JSToken.NotEqual:
                case JSToken.StrictEqual:
                case JSToken.StrictNotEqual:
                    // always returns a boolean
                    return PrimitiveType.Boolean;

                case JSToken.PlusAssign:
                case JSToken.Plus:
                    // if either operand is known to be a string, then the result type is a string.
                    // otherwise the result is numeric if both types are known.
                    leftType = Operand1.FindPrimitiveType();
                    rightType = Operand2.FindPrimitiveType();

                    return (leftType == PrimitiveType.String || rightType == PrimitiveType.String)
                        ? PrimitiveType.String
                        : (leftType != PrimitiveType.Other && rightType != PrimitiveType.Other
                            ? PrimitiveType.Number
                            : PrimitiveType.Other);

                case JSToken.LogicalAnd:
                case JSToken.LogicalOr:
                    // these two are special. They return either the left or the right operand
                    // (depending on their values), so unless they are both known types AND the same,
                    // then we can't know for sure.
                    leftType = Operand1.FindPrimitiveType();
                    if (leftType != PrimitiveType.Other)
                    {
                        if (leftType == Operand2.FindPrimitiveType())
                        {
                            // they are both the same and neither is unknown
                            return leftType;
                        }
                    }

                    // if we get here, then we don't know the type
                    return PrimitiveType.Other;

                default:
                    // shouldn't get here....
                    return PrimitiveType.Other;
            }
        }

        public override IEnumerable<AstNode> Children
        {
            get
            {
                return EnumerateNonNullNodes(Operand1, Operand2);
            }
        }

        public override void Accept(IVisitor visitor)
        {
            if (visitor != null)
            {
                visitor.Visit(this);
            }
        }

        public override bool ReplaceChild(AstNode oldNode, AstNode newNode)
        {
            if (Operand1 == oldNode)
            {
                Operand1 = newNode;
                return true;
            }
            if (Operand2 == oldNode)
            {
                Operand2 = newNode;
                return true;
            }
            return false;
        }

        public override AstNode LeftHandSide
        {
            get
            {
                // the operand1 is on the left
                return Operand1.LeftHandSide;
            }
        }

        public void SwapOperands()
        {
            // swap the operands -- we don't need to go through ReplaceChild 
            // because we don't need to change the Parent pointers 
            // or anything like that.
            AstNode temp = Operand1;
            Operand1 = Operand2;
            Operand2 = temp;
        }

        public override bool IsEquivalentTo(AstNode otherNode)
        {
            // a binary operator is equivalent to another binary operator if the operator is the same and
            // both operands are also equivalent
            var otherBinary = otherNode as BinaryOperator;
            return otherBinary != null
                && OperatorToken == otherBinary.OperatorToken
                && Operand1.IsEquivalentTo(otherBinary.Operand1)
                && Operand2.IsEquivalentTo(otherBinary.Operand2);
        }

        public bool IsAssign
        {
            get
            {
                switch(OperatorToken)
                {
                    case JSToken.Assign:
                    case JSToken.PlusAssign:
                    case JSToken.MinusAssign:
                    case JSToken.MultiplyAssign:
                    case JSToken.DivideAssign:
                    case JSToken.ModuloAssign:
                    case JSToken.BitwiseAndAssign:
                    case JSToken.BitwiseOrAssign:
                    case JSToken.BitwiseXorAssign:
                    case JSToken.LeftShiftAssign:
                    case JSToken.RightShiftAssign:
                    case JSToken.UnsignedRightShiftAssign:
                        return true;

                    default:
                        return false;
                }
            }
        }

        public override string GetFunctionNameGuess(AstNode target)
        {
            var nextNode = target == Operand1 ? Operand2 : Operand1;
            return nextNode == null ? string.Empty : nextNode.GetFunctionNameGuess(this);
        }
    }
}
