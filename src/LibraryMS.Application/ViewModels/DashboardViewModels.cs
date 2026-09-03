namespace LibraryMS.Application.ViewModels;

public class DashboardStatsViewModel
{
    public int TotalBooks       { get; set; }
    public int TotalStudents    { get; set; }
    public int TotalEmployees   { get; set; }
    public int BooksIssued      { get; set; }   // currently out
    public int OverdueBooks     { get; set; }
    public int TotalPurchases   { get; set; }
    public int LowStockBooks    { get; set; }   // RemainingQty == 0
    public int TotalCategories  { get; set; }
}
