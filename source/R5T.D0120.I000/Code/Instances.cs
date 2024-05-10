using System;


namespace R5T.D0120.I000
{
    public static class Instances
    {
        public static H0001.IConnectionStrings ConnectionStrings => H0001.ConnectionStrings.Instance;
        public static L0066.IGuidOperator GuidOperator => L0066.GuidOperator.Instance;
        public static L0066.IHashSetOperator HashSetOperator => L0066.HashSetOperator.Instance;
        public static IProjectDescriptorOperator ProjectDescriptorOperator => I000.ProjectDescriptorOperator.Instance;
        public static IServicesOperator ServicesOperator => I000.ServicesOperator.Instance;
        public static L0089.IWasFoundOperator WasFoundOperator => L0089.WasFoundOperator.Instance;
    }
}