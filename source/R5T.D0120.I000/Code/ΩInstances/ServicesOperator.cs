using System;


namespace R5T.D0120.I000
{
    public class ServicesOperator : IServicesOperator
    {
        #region Infrastructure

        public static IServicesOperator Instance { get; } = new ServicesOperator();


        private ServicesOperator()
        {
        }

        #endregion
    }
}
