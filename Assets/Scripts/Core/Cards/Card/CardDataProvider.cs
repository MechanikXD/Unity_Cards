using System.Collections.Generic;
using System.Text;
using Core.Cards.Card.Data;
using Core.Cards.Card.Effects;
using UnityEngine;

namespace Core.Cards.Card
{
    public static class CardDataProvider
    {
        public static CardDataBank DataBank => Resources.Load<CardDataBank>("Card Data");
        
        // TODO: Add actual sprites and correct path
        private readonly static Dictionary<CardAffinity, Sprite> AffinitySprites =
            new Dictionary<CardAffinity, Sprite>
            {
                [CardAffinity.Destruction] = Resources.Load<Sprite>(""),
                [CardAffinity.Machinery] = Resources.Load<Sprite>(""),
                [CardAffinity.Nature] = Resources.Load<Sprite>(""),
                [CardAffinity.Space] = Resources.Load<Sprite>(""),
                [CardAffinity.Spirit] = Resources.Load<Sprite>(""),
                [CardAffinity.Wildcard] = Resources.Load<Sprite>("")
            };

        private readonly static Dictionary<TriggerType, string> EffectPrefixes =
            new Dictionary<TriggerType, string>
            {
                [TriggerType.CombatStart] = "[Combat Start]",
                [TriggerType.OnHit] = "[On Hit]",
                [TriggerType.TurnEnd] = "[Next Turn]"
            };
        
        public static Sprite GetAffinitySprite(CardAffinity affinity) => AffinitySprites[affinity];

        public static string AttackToString(Vector2Int attackRange, bool insertSpaces = true)
        {
            return insertSpaces
                ? $"{attackRange.x} - {attackRange.y}"
                : $"{attackRange.x}-{attackRange.y}";
        }

        public static string MakeDescription(CardData data)
        {
            var stringBuilder = new StringBuilder();
            foreach (var effect in data.Effects)
            {
                var description = MakeDescription(effect.Value, EffectPrefixes[effect.Key]);
                if (!string.IsNullOrEmpty(description))
                {
                    stringBuilder.Append(description);
                }
            }
            
            return stringBuilder.ToString();
        }
        
        public static string MakeDescription(CardEffect[] effects, string prefix)
        {
            if (effects == null || effects.Length == 0) return string.Empty;

            var stringBuilder = new StringBuilder();
            foreach (CardEffect effect in effects)
            {
                stringBuilder.Append(MakeDescription(effect, prefix));
            }
            
            return stringBuilder.ToString();
        }
        
        public static string MakeDescription(CardEffect effects, string prefix)
        {
            return $"{prefix} {effects.Description}\n";
        }
    }
}