
using DevExpress.Data.Filtering;
using System.ComponentModel;
using System;
using System.Collections.Generic;

namespace AB
{
    public class FilterCriteriaVisitor : IClientCriteriaVisitor
    {

        public List<string> Values { get; set; } = new List<string>();


        public void Visit(OperandProperty theOperand)
        {


        }

        public void Visit(BetweenOperator theOperator)
        {
            theOperator.TestExpression.Accept(this);
        }

        public void Visit(GroupOperator theOperator)
        {
            foreach (var operand in theOperator.Operands)
                operand.Accept(this);
        }

        public void Visit(InOperator theOperator)
        {
            LeftOperator(theOperator);
        }

        private void LeftOperator(InOperator theOperator)
        {
            theOperator.LeftOperand.Accept(this);
        }

        public void Visit(UnaryOperator theOperator)
        {
            theOperator.Operand.Accept(this);
        }

        public void Visit(BinaryOperator theOperator)
        {
            Values.Add(theOperator.LeftOperand.ToString() + " " + theOperator.RightOperand.ToString());
            theOperator.LeftOperand.Accept(this);
            theOperator.RightOperand.Accept(this);
        }

        public void Visit(JoinOperand theOperand)
        {
            theOperand.AggregatedExpression.Accept(this);
            theOperand.Condition.Accept(this);
        }

        public void Visit(FunctionOperator theOperator)
        {
            foreach (var operand in theOperator.Operands)
                operand.Accept(this);
        }

        public void Visit(AggregateOperand theOperand)
        {
            theOperand.AggregatedExpression.Accept(this);
            theOperand.Condition.Accept(this);
        }

        public void Visit(OperandValue theOperand) { }
    }
}
