namespace ChatServer.Utils
{
    public static class GiftGenerator
    {
        private static readonly string[] Gifts = [
            "불타는 보물상자",
            "고대의 반지",
            "저주받은 상자",
            "마력의 수정",
            "빛나는 깃털",
            "망자의 손가락",
            "지옥의 구슬",
            "영혼의 돌"
        ];

        private static readonly Random _random = new();

        public static string GetRandomGift()
        {
            return Gifts[_random.Next(Gifts.Length)];
        }
    }
}
