namespace Osnova.StaticRazorPages;

public class ExecutionStatistics
{
    private int _pagesTypes;
    private int _failedPages;
    private int _totalPages;

    public int TotalPages => _totalPages;

    public int FailedPages => _failedPages;

    public int PagesTypes => _pagesTypes;

    public void MarkPageType(int number = 1)
    {
        Interlocked.Add(ref _pagesTypes, number);
    }

    public void MarkFailed(int number = 1)
    {
        Interlocked.Add(ref _failedPages, number);
    }

    public void MarkPage(int number = 1)
    {
        Interlocked.Add(ref _totalPages, number);
    }
}