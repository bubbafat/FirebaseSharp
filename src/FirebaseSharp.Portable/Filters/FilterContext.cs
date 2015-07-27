namespace FirebaseSharp.Portable.Filters
{
    internal interface IFilterContext
    {
        string FilterColumn { get; set; }
    }

    internal class FilterContext : IFilterContext
    {
        public string FilterColumn { get; set; }
    }
}