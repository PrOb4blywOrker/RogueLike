namespace Completed {
    public static class Extensions {
        public static void Nullify<T>(this T[,] array) where T : class {
            var n = array.GetLength(0);
            var m = array.GetLength(1);
            for (int i = 0; i < n; i++) {
                for (int j = 0; j < m; j++) {
                    array[i, j] = null;
                }
            }
        }
    }
}