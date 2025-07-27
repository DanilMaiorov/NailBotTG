namespace NailBot.Helpers
{
    public static class EnumerableExtension
    {
        /// <summary>
        /// Возвращает подмножество элементов из последовательности для указанной пачки
        /// </summary>
        /// <typeparam name="T">Тип элементов</typeparam>
        /// <param name="source">Исходная последовательность</param>
        /// <param name="batchSize">Размер пачки</param>
        /// <param name="batchNumber">Номер пачки (начиная с 0)</param>
        /// <returns>Элементы указанной пачки</returns>
        public static IEnumerable<T> GetBatchByNumber<T>(this IEnumerable<T> source, int batchSize, int batchNumber)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (batchSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(batchSize), "Размер пачки должен быть положительным числом");

            return source.Skip(batchNumber * batchSize).Take(batchSize);
        }

        /// <summary>
        /// Преобразует коллекцию в список пар "ключ-значение" только для чтения
        /// </summary>
        /// <param name="source">Исходная коллекция</param>
        /// <param name="keySelector">Функция получения ключа из элемента</param>
        /// <param name="valueSelector">Функция получения значения из элемента</param>
        /// <returns>Неизменяемый список пар "ключ-значение"</returns>
        public static IReadOnlyList<KeyValuePair<string, string>> ToReadOnlyKeyValueList<T>(
            this IEnumerable<T> source,
            Func<T, string> keySelector,
            Func<T, string> valueSelector)
        {
            return source
                .Select(item => new KeyValuePair<string, string>(keySelector(item), valueSelector(item)))
                .ToList()
                .AsReadOnly();
        }
    }
}
