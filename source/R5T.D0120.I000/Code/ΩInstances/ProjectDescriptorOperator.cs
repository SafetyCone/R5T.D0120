using System;


namespace R5T.D0120.I000
{
    public class ProjectDescriptorOperator : IProjectDescriptorOperator
    {
        #region Infrastructure

        public static IProjectDescriptorOperator Instance { get; } = new ProjectDescriptorOperator();


        private ProjectDescriptorOperator()
        {
        }

        #endregion
    }
}
