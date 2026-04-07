/// <summary>
/// 사과 숫자(1~9) 그리드 생성. Easy는 가로로 합이 10인 쌍을 자주 배치하고,
/// Normal은 무작위 후 인접 쌍이 합 10이 되도록 일부 보강한다.
/// </summary>
public static class AppleGridGenerator
{
    private static readonly int[][] Sum10Pairs =
    {
        new[] { 1, 9 }, new[] { 2, 8 }, new[] { 3, 7 }, new[] { 4, 6 }, new[] { 5, 5 }
    };

    public static void FillGrid(int[,] grid, AppleGameStageData stage, System.Random rng)
    {
        int rows = grid.GetLength(0);
        int cols = grid.GetLength(1);
        bool easy = stage != null && stage.useEasyFillRules;

        if (easy)
        {
            FillEasy(grid, rows, cols, rng);
        }
        else
        {
            FillNormal(grid, rows, cols, rng);
        }
    }

    private static void FillEasy(int[,] grid, int rows, int cols, System.Random rng)
    {
        for (int r = 0; r < rows; r++)
        {
            int c = 0;
            while (c < cols)
            {
                if (c + 1 < cols && rng.NextDouble() < 0.78)
                {
                    int[] p = Sum10Pairs[rng.Next(Sum10Pairs.Length)];
                    grid[r, c] = p[0];
                    grid[r, c + 1] = p[1];
                    c += 2;
                }
                else
                {
                    grid[r, c] = rng.Next(1, 10);
                    c++;
                }
            }
        }
    }

    private static void FillNormal(int[,] grid, int rows, int cols, System.Random rng)
    {
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                grid[r, c] = rng.Next(1, 10);
            }
        }

        for (int pass = 0; pass < 2; pass++)
        {
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols - 1; c++)
                {
                    if (rng.NextDouble() < 0.32)
                    {
                        int[] p = Sum10Pairs[rng.Next(Sum10Pairs.Length)];
                        grid[r, c] = p[0];
                        grid[r, c + 1] = p[1];
                        c++;
                    }
                }
            }
        }
    }
}
