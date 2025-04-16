using ChatServer.Models;

namespace ChatServer.Utils
{
    public static class GiftGenerator
    {
        private static readonly List<Item> _items = [
            new Item("cracked_skull", "금 간 해골", 1),
            new Item("demonic_eye", "악마의 눈알", 2),
            new Item("bloodied_scroll", "피로 얼룩진 주문서", 3),
            new Item("black_feather", "검은 깃털", 4),
            new Item("shard_of_fear", "공포의 파편", 5),
            new Item("eternal_nail", "영원의 못", 6),
            new Item("cursed_bone", "저주받은 뼈", 7),
            new Item("shade_flame", "그림자 불꽃", 8),
            new Item("ancient_idol", "고대의 우상", 10),
            new Item("soul_flower", "영혼의 꽃", 11),
            new Item("chaos_dust", "혼돈의 가루", 12),
            new Item("dried_heart", "말라붙은 심장", 13),
            new Item("frozen_echo", "얼어붙은 메아리", 14),
            new Item("ghost_ink", "유령의 잉크", 16),
            new Item("blood_oath", "피의 맹세", 18),
            new Item("dark_relic", "어둠의 유물", 20),
            new Item("sigil_of_war", "전쟁의 인장", 22),
            new Item("stone_of_grief", "슬픔의 돌", 25),
            new Item("twilight_sigil", "황혼의 인장", 28),
            new Item("bone_diadem", "해골 왕관", 30),
            new Item("echo_of_agony", "고통의 메아리", 35),
            new Item("void_fragment", "공허의 조각", 40),
            new Item("mirror_of_fate", "운명의 거울", 45),
            new Item("soulbound_core", "영혼결속 핵", 50),
            new Item("eye_of_the_gods", "신의 눈", 60),
            new Item("eternal_ember", "영원의 불씨", 70),
            new Item("wrath_gem", "분노의 보석", 75),
            new Item("celestial_claw", "천상의 발톱", 80),
            new Item("heart_of_the_void", "공허의 심장", 90),
            new Item("crown_of_the_fallen", "몰락자의 왕관", 100)
        ];

        private static readonly Random _random = new();

        public static Item GetRandomGift()
        {
            var weightedItems = _items.Select(item =>
            {
                if (item.Value == 0)
                {
                    throw new ArgumentException("아이템의 가치 값은 0일 수 없습니다.");
                }
                return new { Item = item, Weight = 1.0 / item.Value };
            });

            var totalWeight = weightedItems.Sum(item => item.Weight);
            var roll = _random.NextDouble() * totalWeight;

            double cumulative = 0.0;
            foreach (var entry in weightedItems)
            {
                cumulative += entry.Weight;
                if (cumulative >= roll)
                {
                    return entry.Item;
                }
            }

            return _items[0];
        }
    }
}
