public class SudokuCreator
{
    private static SudokuCreator instance;
    private int[] _sudokuGrid = new int[81];
    private bool _isCompleted;

    public static SudokuCreator GetObject()
    {
        if (instance == null)
            instance = new SudokuCreator();

        return instance;
    }

    public int[] GetRandomSudoku()
    {
        _isCompleted = false;
        CreateSudoku(0);
        return _sudokuGrid;
    }

    private void CreateSudoku(int ind)
    {
        if (ind > 80)
        {
            _isCompleted = true;
            return;
        }
        int a = UnityEngine.Random.Range(1, 10);
        int i = a + 1;
        if (i > 9) i = 1;
        while (!_isCompleted && i != a)
        {
            _sudokuGrid[ind] = i;
            if (Check(ind)) CreateSudoku(ind + 1);
            i++;
            if (i > 9) i = 1;
        }
    }

    private bool Check(int ind)
    {
        int i;
        for (i = ind - 1; i >= 0 && i / 9 == ind / 9; i--)
        {
            if (_sudokuGrid[ind] == _sudokuGrid[i]) return false;
        }

        for (i = ind - 9; i >= 0; i -= 9)
        {
            if (_sudokuGrid[ind] == _sudokuGrid[i]) return false;
        }

        for (i = ind - 1;; i--)
        {
            if (i % 3 == 2) i -= 6;
            if (i < 0 || i / 27 != ind / 27) break;

            if (_sudokuGrid[ind] == _sudokuGrid[i]) return false;
        }
        return true;
    }
}