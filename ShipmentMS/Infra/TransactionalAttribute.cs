using System;
namespace ShipmentMS.Infra
{
    [AttributeUsage(AttributeTargets.Method)]
    public class TransactionalAttribute : Attribute
    {

        public System.Data.IsolationLevel IsolationLevel { get; }

        public TransactionalAttribute(System.Data.IsolationLevel IsolationLevel = System.Data.IsolationLevel.ReadCommitted)
        {
            this.IsolationLevel = IsolationLevel;
        }

    }
}

