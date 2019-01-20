using GameXML;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colony_Ship_Horizon
{
    // struct for npc data, content is loaded here and later accessed when creating new npcs later in game
    public struct NPC_Type
    {
        public Texture2D _npcJumpingRightSheet;
        public Texture2D _npcStandingRightSheet;
        public Texture2D _npcWalkingRightSheet;
        public Texture2D _npcAttackingRightSheet;
        public SpriteMap _npcJumpingRight;
        public SpriteMap _npcStandingRight;
        public SpriteMap _npcWalkingRight;
        public SpriteMap _npcAttackingRight;

        public Dictionary<string, Texture2D> _actionSheets;
        public Dictionary<string, SpriteMap> _actionMaps;

        public Texture2D _projectileTexture;

        public Animator npcDeathAnim;

        public NPC_Type(Texture2D npcJumpingRightSheet, Texture2D npcStandingRightSheet, Texture2D npcWalkingRightSheet,
            Texture2D npcAttackingRightSheet, SpriteMap npcJumpingRight, SpriteMap npcStandingRight, SpriteMap npcWalkingRight, SpriteMap npcAttackingRight,
            Texture2D projectileTexture, Texture2D npcDeathSheet, SpriteMap npcDeath, Dictionary<string, Texture2D> actionSheets = null,
            Dictionary<string, SpriteMap> actionMaps = null)
        {
            _npcJumpingRightSheet = npcJumpingRightSheet;
            _npcStandingRightSheet = npcStandingRightSheet;
            _npcWalkingRightSheet = npcWalkingRightSheet;
            _npcAttackingRightSheet = npcAttackingRightSheet;
            _npcJumpingRight = npcJumpingRight;
            _npcStandingRight = npcStandingRight;
            _npcWalkingRight = npcWalkingRight;
            _npcAttackingRight = npcAttackingRight;
            _projectileTexture = projectileTexture;
            _actionSheets = actionSheets;
            _actionMaps = actionMaps;
            npcDeathAnim = new Animator(npcDeath, npcDeathSheet, 50, false, true);
        }
    }
}
