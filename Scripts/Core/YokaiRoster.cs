using System;

// Shared companion catalog for the Party screen and all seven story battles.
// Indexes are stable across saves: never reorder the entries.
public static class YokaiRoster
{
    private static readonly string[] Names =
    {
        "WHISPER", "TATTLETELL", "CHEEKSQUEEK", "DRAGGIE",
        "ENMA", "WOBBLEWOK", "ORCANOS"
    };

    // Characters/ filenames follow upload order, not the M1-M7 Yokai names.
    private static readonly string[] PortraitPaths =
    {
        "res://Characters/one.png", "res://Characters/two.png",
        "res://Characters/six.jpg", "res://Characters/three.png",
        "res://Characters/seven.jpg", "res://Characters/five.png",
        "res://Characters/four.png"
    };

    private static readonly string[] BattleSpritePaths =
    {
        "res://Sprites/M1_Front.png",
        "res://Sprites/YokaiEnemy/M2_Front.png",
        "res://Sprites/YokaiEnemy/M3_Front.png",
        "res://Sprites/YokaiEnemy/M4_Front.png",
        "res://Sprites/YokaiEnemy/M5_Front.png",
        "res://Sprites/YokaiEnemy/M6_Front.png",
        "res://Sprites/YokaiEnemy/M7_Front.png"
    };

    private static readonly string[] NormalSkillPaths =
    {
        "res://Sprites/m1s-slap.png",
        "res://Sprites/skills/m2s_peckq.png",
        "res://Sprites/skills/m3s_bump.png",
        "res://Sprites/skills/m4s_twhip.png",
        "res://Sprites/skills/m5s_fburst.png",
        "res://Sprites/skills/m6s_wave.png",
        "res://Sprites/skills/m7s_earthquake.png"
    };

    private static readonly string[] UltimateSkillPaths =
    {
        "res://Sprites/m1s_gpunch.png",
        "res://Sprites/skills/m2s_gblast.png",
        "res://Sprites/skills/m3s_csqueak.png",
        "res://Sprites/skills/m4s_fball.png",
        "res://Sprites/skills/m5s_rstreak.png",
        "res://Sprites/skills/m6s_dblast.png",
        "res://Sprites/skills/m7s_osmash.png"
    };

    // Small differences in base damage keep companions distinct without
    // replacing the existing stage and friendship balancing.
    private static readonly int[] NormalPower = { 8, 9, 8, 10, 9, 9, 10 };
    private static readonly int[] UltimatePower = { 16, 17, 18, 16, 19, 18, 20 };
    private static readonly string[] NormalMoveNames =
    {
        "Slap", "Peck", "Bump", "Tail Whip", "Flame Burst", "Wave", "Earthquake"
    };
    private static readonly string[] UltimateMoveNames =
    {
        "Ghost Punch", "Ghost Blast", "Cheek Squeak", "Fireball",
        "Royal Strike", "Dark Blast", "Omega Smash"
    };

    private static int SafeIndex(int index) => Math.Clamp(index, 0, Names.Length - 1);

    public static string Name(int index) => Names[SafeIndex(index)];
    public static string Portrait(int index) => PortraitPaths[SafeIndex(index)];
    public static string BattleSprite(int index) => BattleSpritePaths[SafeIndex(index)];
    public static string NormalSkill(int index) => NormalSkillPaths[SafeIndex(index)];
    public static string UltimateSkill(int index) => UltimateSkillPaths[SafeIndex(index)];
    public static int NormalBaseDamage(int index) => NormalPower[SafeIndex(index)];
    public static int UltimateBaseDamage(int index) => UltimatePower[SafeIndex(index)];
    public static string NormalMoveName(int index) => NormalMoveNames[SafeIndex(index)];
    public static string UltimateMoveName(int index) => UltimateMoveNames[SafeIndex(index)];
}
