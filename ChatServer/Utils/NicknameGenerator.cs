namespace ChatServer.Utils
{
    public static class NicknameGenerator
    {
        private static readonly string[] Prefixes = [
            "타락한", "고통받는", "불멸의", "망각의", "피로 물든", "저주받은", "지옥의", "광기의", "암흑의", "영원의"
        ];

        private static readonly string[] Suffixes = [
            "악마", "영혼", "전사", "파수꾼", "복수자", "사냥꾼", "주술사", "망자", "검은 심장", "영혼 사슬"
        ];

        private static readonly Random _random = new();

        public static string Generate()
        {
            var prefix = Prefixes[_random.Next(Prefixes.Length)];
            var suffix = Suffixes[_random.Next(Suffixes.Length)];
            var number = _random.Next(0, 10000);
            return $"{prefix} {suffix}${number:D4}";
        }
    }
}
