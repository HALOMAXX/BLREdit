using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BLREdit.Game;

public sealed class BLRKeyBindings
{
    [JsonPropertyName("GBA_Chat")]
    public BLRKeyBind Chat { get; set; } = new BLRKeyBind("Enter", "Slash");
    [JsonPropertyName("GBA_Fire")]
    public BLRKeyBind Fire { get; set; } = new BLRKeyBind("LeftMouseButton", "Quote");
    [JsonPropertyName("GBA_HoldCrouch")]
    public BLRKeyBind HoldCrouch { get; set; } = new BLRKeyBind("LeftControl", "Z");
    [JsonPropertyName("GBA_Jump")]
    public BLRKeyBind Jump { get; set; } = new BLRKeyBind("SpaceBar", "K");
    [JsonPropertyName("GBA_LastWeapon")]
    public BLRKeyBind LastWeapon { get; set; } = new BLRKeyBind("Z", "Y");
    [JsonPropertyName("GBA_Melee")]
    public BLRKeyBind Melee { get; set; } = new BLRKeyBind("F", "MiddleMouseButton");
    [JsonPropertyName("GBA_MoveBackward")]
    public BLRKeyBind MoveBackward { get; set; } = new BLRKeyBind("S", "Down");
    [JsonPropertyName("GBA_MoveForward")]
    public BLRKeyBind MoveForward { get; set; } = new BLRKeyBind("W", "Up");
    [JsonPropertyName("GBA_NextWeapon")]
    public BLRKeyBind NextWeapon { get; set; } = new BLRKeyBind("MouseScrollDown", "Add");
    [JsonPropertyName("GBA_PickupWeapon")]
    public BLRKeyBind PickupWeapon { get; set; } = new BLRKeyBind("Q", "P");
    [JsonPropertyName("GBA_PrevWeapon")]
    public BLRKeyBind PrevWeapon { get; set; } = new BLRKeyBind("MouseScrollUp", "Subtract");
    [JsonPropertyName("GBA_QuickGear")]
    public BLRKeyBind QuickGear { get; set; } = new BLRKeyBind("G", "L");
    [JsonPropertyName("GBA_Reload")]
    public BLRKeyBind Reload { get; set; } = new BLRKeyBind("R", "Delete");
    [JsonPropertyName("GBA_SelectPrimaryWeapon")]
    public BLRKeyBind SelectPrimaryWeapon { get; set; } = new BLRKeyBind("One", "NumPadOne");
    [JsonPropertyName("GBA_SelectSecondaryWeapon")]
    public BLRKeyBind SelectSecondaryWeapon { get; set; } = new BLRKeyBind("Two", "NumPadTwo");
    [JsonPropertyName("GBA_SelectTactical")]
    public BLRKeyBind SelectTactical { get; set; } = new BLRKeyBind("C", "M");
    [JsonPropertyName("GBA_Sprint")]
    public BLRKeyBind Sprint { get; set; } = new BLRKeyBind("LeftShift", "RightShift");
    [JsonPropertyName("GBA_StrafeLeft")]
    public BLRKeyBind StrafeLeft { get; set; } = new BLRKeyBind("A", "Left");
    [JsonPropertyName("GBA_StrafeRight")]
    public BLRKeyBind StrafeRight { get; set; } = new BLRKeyBind("D", "Right");
    [JsonPropertyName("GBA_SwitchGear1")]
    public BLRKeyBind SwitchGear1 { get; set; } = new BLRKeyBind("Three", "NumPadThree");
    [JsonPropertyName("GBA_SwitchGear2")]
    public BLRKeyBind SwitchGear2 { get; set; } = new BLRKeyBind("Four", "NumPadFour");
    [JsonPropertyName("GBA_SwitchGear3")]
    public BLRKeyBind SwitchGear3 { get; set; } = new BLRKeyBind("Five", "NumPadFive");
    [JsonPropertyName("GBA_SwitchGear4")]
    public BLRKeyBind SwitchGear4 { get; set; } = new BLRKeyBind("Six", "NumPadSix");
    [JsonPropertyName("GBA_Taunt")]
    public BLRKeyBind Taunt { get; set; } = new BLRKeyBind("T", "NumPadZero");
    [JsonPropertyName("GBA_ToggleVisor")]
    public BLRKeyBind ToggleVisor { get; set; } = new BLRKeyBind("V", "B");
    [JsonPropertyName("GBA_ToggleZoom")]
    public BLRKeyBind ToggleZoom { get; set; } = new BLRKeyBind("RightMouseButton", "RightControl");
    [JsonPropertyName("GBA_UseObject")]
    public BLRKeyBind UseObject { get; set; } = new BLRKeyBind("E", "BackSpace");
}

public sealed class BLRKeyBind : INotifyPropertyChanged
{
    #region Events
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
    #endregion Events

    [JsonPropertyName("alternate")]
    public string Alternate { get { return alternate; } set { alternate = value; OnPropertyChanged(); } }
    private string alternate = "Enter";
    [JsonPropertyName("primary")]
    public string Primary { get { return primary; } set { primary = value; OnPropertyChanged(); } }
    private string primary = "Slash";

    public BLRKeyBind() { }
    public BLRKeyBind(string primary, string alternate)
    {
        Primary = primary;
        Alternate = alternate;
    }
}

//public enum BLRKey
//{
//    //
//    // Summary:
//    //     No key pressed.
//    None = 0,
//    //
//    // Summary:
//    //     The Cancel key.
//    Cancel = 1,
//    /// <summary>
//    /// The Backspace key.
//    /// </summary>
//    BackSpace = 2,
//    //
//    // Summary:
//    //     The Tab key.
//    Tab = 3,
//    //
//    // Summary:
//    //     The Linefeed key.
//    LineFeed = 4,
//    //
//    // Summary:
//    //     The Clear key.
//    Clear = 5,
//    //
//    // Summary:
//    //     The Return key.
//    Return = 6,
//    //
//    // Summary:
//    //     The Enter key.
//    Enter = 6,
//    //
//    // Summary:
//    //     The Pause key.
//    Pause = 7,
//    //
//    // Summary:
//    //     The Caps Lock key.
//    Capital = 8,
//    //
//    // Summary:
//    //     The Caps Lock key.
//    CapsLock = 8,
//    //
//    // Summary:
//    //     The IME Kana mode key.
//    KanaMode = 9,
//    //
//    // Summary:
//    //     The IME Hangul mode key.
//    HangulMode = 9,
//    //
//    // Summary:
//    //     The IME Junja mode key.
//    JunjaMode = 10,
//    //
//    // Summary:
//    //     The IME Final mode key.
//    FinalMode = 11,
//    //
//    // Summary:
//    //     The IME Hanja mode key.
//    HanjaMode = 12,
//    //
//    // Summary:
//    //     The IME Kanji mode key.
//    KanjiMode = 12,
//    //
//    // Summary:
//    //     The ESC key.
//    Escape = 13,
//    //
//    // Summary:
//    //     The IME Convert key.
//    ImeConvert = 14,
//    //
//    // Summary:
//    //     The IME NonConvert key.
//    ImeNonConvert = 0xF,
//    //
//    // Summary:
//    //     The IME Accept key.
//    ImeAccept = 0x10,
//    //
//    // Summary:
//    //     The IME Mode change request.
//    ImeModeChange = 17,
//    //
//    // Summary:
//    //     The Spacebar key.
//    Space = 18,
//    //
//    // Summary:
//    //     The Page Up key.
//    Prior = 19,
//    //
//    // Summary:
//    //     The Page Up key.
//    PageUp = 19,
//    //
//    // Summary:
//    //     The Page Down key.
//    Next = 20,
//    //
//    // Summary:
//    //     The Page Down key.
//    PageDown = 20,
//    //
//    // Summary:
//    //     The End key.
//    End = 21,
//    //
//    // Summary:
//    //     The Home key.
//    Home = 22,
//    //
//    // Summary:
//    //     The Left Arrow key.
//    Left = 23,
//    //
//    // Summary:
//    //     The Up Arrow key.
//    Up = 24,
//    //
//    // Summary:
//    //     The Right Arrow key.
//    Right = 25,
//    //
//    // Summary:
//    //     The Down Arrow key.
//    Down = 26,
//    //
//    // Summary:
//    //     The Select key.
//    Select = 27,
//    //
//    // Summary:
//    //     The Print key.
//    Print = 28,
//    //
//    // Summary:
//    //     The Execute key.
//    Execute = 29,
//    //
//    // Summary:
//    //     The Print Screen key.
//    Snapshot = 30,
//    //
//    // Summary:
//    //     The Print Screen key.
//    PrintScreen = 30,
//    //
//    // Summary:
//    //     The Insert key.
//    Insert = 0x1F,
//    //
//    // Summary:
//    //     The Delete key.
//    Delete = 0x20,
//    //
//    // Summary:
//    //     The Help key.
//    Help = 33,
//    //
//    // Summary:
//    //     The 0 (zero) key.
//    Zero = 34,
//    //
//    // Summary:
//    //     The 1 (one) key.
//    One = 35,
//    //
//    // Summary:
//    //     The 2 key.
//    Two = 36,
//    //
//    // Summary:
//    //     The 3 key.
//    Three = 37,
//    //
//    // Summary:
//    //     The 4 key.
//    Four = 38,
//    //
//    // Summary:
//    //     The 5 key.
//    Five = 39,
//    //
//    // Summary:
//    //     The 6 key.
//    Six = 40,
//    //
//    // Summary:
//    //     The 7 key.
//    Seven = 41,
//    //
//    // Summary:
//    //     The 8 key.
//    Eight = 42,
//    //
//    // Summary:
//    //     The 9 key.
//    Nine = 43,
//    //
//    // Summary:
//    //     The A key.
//    A = 44,
//    //
//    // Summary:
//    //     The B key.
//    B = 45,
//    //
//    // Summary:
//    //     The C key.
//    C = 46,
//    //
//    // Summary:
//    //     The D key.
//    D = 47,
//    //
//    // Summary:
//    //     The E key.
//    E = 48,
//    //
//    // Summary:
//    //     The F key.
//    F = 49,
//    //
//    // Summary:
//    //     The G key.
//    G = 50,
//    //
//    // Summary:
//    //     The H key.
//    H = 51,
//    //
//    // Summary:
//    //     The I key.
//    I = 52,
//    //
//    // Summary:
//    //     The J key.
//    J = 53,
//    //
//    // Summary:
//    //     The K key.
//    K = 54,
//    //
//    // Summary:
//    //     The L key.
//    L = 55,
//    //
//    // Summary:
//    //     The M key.
//    M = 56,
//    //
//    // Summary:
//    //     The N key.
//    N = 57,
//    //
//    // Summary:
//    //     The O key.
//    O = 58,
//    //
//    // Summary:
//    //     The P key.
//    P = 59,
//    //
//    // Summary:
//    //     The Q key.
//    Q = 60,
//    //
//    // Summary:
//    //     The R key.
//    R = 61,
//    //
//    // Summary:
//    //     The S key.
//    S = 62,
//    //
//    // Summary:
//    //     The T key.
//    T = 0x3F,
//    //
//    // Summary:
//    //     The U key.
//    U = 0x40,
//    //
//    // Summary:
//    //     The V key.
//    V = 65,
//    //
//    // Summary:
//    //     The W key.
//    W = 66,
//    //
//    // Summary:
//    //     The X key.
//    X = 67,
//    //
//    // Summary:
//    //     The Y key.
//    Y = 68,
//    //
//    // Summary:
//    //     The Z key.
//    Z = 69,
//    //
//    // Summary:
//    //     The left Windows logo key (Microsoft Natural Keyboard).
//    LWin = 70,
//    //
//    // Summary:
//    //     The right Windows logo key (Microsoft Natural Keyboard).
//    RWin = 71,
//    //
//    // Summary:
//    //     The Application key (Microsoft Natural Keyboard).
//    Apps = 72,
//    //
//    // Summary:
//    //     The Computer Sleep key.
//    Sleep = 73,
//    //
//    // Summary:
//    //     The 0 key on the numeric keypad.
//    NumPadZero = 74,
//    //
//    // Summary:
//    //     The 1 key on the numeric keypad.
//    NumPadOne = 75,
//    //
//    // Summary:
//    //     The 2 key on the numeric keypad.
//    NumPadTwo = 76,
//    //
//    // Summary:
//    //     The 3 key on the numeric keypad.
//    NumPadThree = 77,
//    //
//    // Summary:
//    //     The 4 key on the numeric keypad.
//    NumPadFour = 78,
//    //
//    // Summary:
//    //     The 5 key on the numeric keypad.
//    NumPadFive = 79,
//    //
//    // Summary:
//    //     The 6 key on the numeric keypad.
//    NumPadSix = 80,
//    //
//    // Summary:
//    //     The 7 key on the numeric keypad.
//    NumPadSeven = 81,
//    //
//    // Summary:
//    //     The 8 key on the numeric keypad.
//    NumPadEight = 82,
//    //
//    // Summary:
//    //     The 9 key on the numeric keypad.
//    NumPadNine = 83,
//    //
//    // Summary:
//    //     The Multiply key.
//    Multiply = 84,
//    //
//    // Summary:
//    //     The Add key.
//    Add = 85,
//    //
//    // Summary:
//    //     The Separator key.
//    Separator = 86,
//    //
//    // Summary:
//    //     The Subtract key.
//    Subtract = 87,
//    //
//    // Summary:
//    //     The Decimal key.
//    Decimal = 88,
//    //
//    // Summary:
//    //     The Divide key.
//    Divide = 89,
//    //
//    // Summary:
//    //     The F1 key.
//    F1 = 90,
//    //
//    // Summary:
//    //     The F2 key.
//    F2 = 91,
//    //
//    // Summary:
//    //     The F3 key.
//    F3 = 92,
//    //
//    // Summary:
//    //     The F4 key.
//    F4 = 93,
//    //
//    // Summary:
//    //     The F5 key.
//    F5 = 94,
//    //
//    // Summary:
//    //     The F6 key.
//    F6 = 95,
//    //
//    // Summary:
//    //     The F7 key.
//    F7 = 96,
//    //
//    // Summary:
//    //     The F8 key.
//    F8 = 97,
//    //
//    // Summary:
//    //     The F9 key.
//    F9 = 98,
//    //
//    // Summary:
//    //     The F10 key.
//    F10 = 99,
//    //
//    // Summary:
//    //     The F11 key.
//    F11 = 100,
//    //
//    // Summary:
//    //     The F12 key.
//    F12 = 101,
//    //
//    // Summary:
//    //     The F13 key.
//    F13 = 102,
//    //
//    // Summary:
//    //     The F14 key.
//    F14 = 103,
//    //
//    // Summary:
//    //     The F15 key.
//    F15 = 104,
//    //
//    // Summary:
//    //     The F16 key.
//    F16 = 105,
//    //
//    // Summary:
//    //     The F17 key.
//    F17 = 106,
//    //
//    // Summary:
//    //     The F18 key.
//    F18 = 107,
//    //
//    // Summary:
//    //     The F19 key.
//    F19 = 108,
//    //
//    // Summary:
//    //     The F20 key.
//    F20 = 109,
//    //
//    // Summary:
//    //     The F21 key.
//    F21 = 110,
//    //
//    // Summary:
//    //     The F22 key.
//    F22 = 111,
//    //
//    // Summary:
//    //     The F23 key.
//    F23 = 112,
//    //
//    // Summary:
//    //     The F24 key.
//    F24 = 113,
//    //
//    // Summary:
//    //     The Num Lock key.
//    NumLock = 114,
//    //
//    // Summary:
//    //     The Scroll Lock key.
//    Scroll = 115,
//    //
//    // Summary:
//    //     The left Shift key.
//    LeftShift = 116,
//    //
//    // Summary:
//    //     The right Shift key.
//    RightShift = 117,
//    //
//    // Summary:
//    //     The left CTRL key.
//    LeftControl = 118,
//    //
//    // Summary:
//    //     The right CTRL key.
//    RightControl = 119,
//    //
//    // Summary:
//    //     The left ALT key.
//    LeftAlt = 120,
//    //
//    // Summary:
//    //     The right ALT key.
//    RightAlt = 121,
//    //
//    // Summary:
//    //     The Browser Back key.
//    BrowserBack = 122,
//    //
//    // Summary:
//    //     The Browser Forward key.
//    BrowserForward = 123,
//    //
//    // Summary:
//    //     The Browser Refresh key.
//    BrowserRefresh = 124,
//    //
//    // Summary:
//    //     The Browser Stop key.
//    BrowserStop = 125,
//    //
//    // Summary:
//    //     The Browser Search key.
//    BrowserSearch = 126,
//    //
//    // Summary:
//    //     The Browser Favorites key.
//    BrowserFavorites = 0x7F,
//    //
//    // Summary:
//    //     The Browser Home key.
//    BrowserHome = 0x80,
//    //
//    // Summary:
//    //     The Volume Mute key.
//    VolumeMute = 129,
//    //
//    // Summary:
//    //     The Volume Down key.
//    VolumeDown = 130,
//    //
//    // Summary:
//    //     The Volume Up key.
//    VolumeUp = 131,
//    //
//    // Summary:
//    //     The Media Next Track key.
//    MediaNextTrack = 132,
//    //
//    // Summary:
//    //     The Media Previous Track key.
//    MediaPreviousTrack = 133,
//    //
//    // Summary:
//    //     The Media Stop key.
//    MediaStop = 134,
//    //
//    // Summary:
//    //     The Media Play Pause key.
//    MediaPlayPause = 135,
//    //
//    // Summary:
//    //     The Launch Mail key.
//    LaunchMail = 136,
//    //
//    // Summary:
//    //     The Select Media key.
//    SelectMedia = 137,
//    //
//    // Summary:
//    //     The Launch Application1 key.
//    LaunchApplication1 = 138,
//    //
//    // Summary:
//    //     The Launch Application2 key.
//    LaunchApplication2 = 139,
//    //
//    // Summary:
//    //     The OEM 1 key.
//    Oem1 = 140,
//    //
//    // Summary:
//    //     The OEM Semicolon key.
//    OemSemicolon = 140,
//    //
//    // Summary:
//    //     The OEM Addition key.
//    OemPlus = 141,
//    //
//    // Summary:
//    //     The OEM Comma key.
//    OemComma = 142,
//    //
//    // Summary:
//    //     The OEM Minus key.
//    OemMinus = 143,
//    //
//    // Summary:
//    //     The OEM Period key.
//    OemPeriod = 144,
//    //
//    // Summary:
//    //     The OEM 2 key.
//    Oem2 = 145,
//    //
//    // Summary:
//    //     The OEM Question key.
//    OemQuestion = 145,
//    //
//    // Summary:
//    //     The OEM 3 key.
//    Oem3 = 146,
//    //
//    // Summary:
//    //     The OEM Tilde key.
//    OemTilde = 146,
//    //
//    // Summary:
//    //     The ABNT_C1 (Brazilian) key.
//    AbntC1 = 147,
//    //
//    // Summary:
//    //     The ABNT_C2 (Brazilian) key.
//    AbntC2 = 148,
//    //
//    // Summary:
//    //     The OEM 4 key.
//    Oem4 = 149,
//    //
//    // Summary:
//    //     The OEM Open Brackets key.
//    OemOpenBrackets = 149,
//    //
//    // Summary:
//    //     The OEM 5 key.
//    Oem5 = 150,
//    //
//    // Summary:
//    //     The OEM Pipe key.
//    OemPipe = 150,
//    //
//    // Summary:
//    //     The OEM 6 key.
//    Oem6 = 151,
//    //
//    // Summary:
//    //     The OEM Close Brackets key.
//    OemCloseBrackets = 151,
//    //
//    // Summary:
//    //     The OEM 7 key.
//    Oem7 = 152,
//    //
//    // Summary:
//    //     The OEM Quotes key.
//    OemQuotes = 152,
//    //
//    // Summary:
//    //     The OEM 8 key.
//    Oem8 = 153,
//    //
//    // Summary:
//    //     The OEM 102 key.
//    Oem102 = 154,
//    //
//    // Summary:
//    //     The OEM Backslash key.
//    OemBackslash = 154,
//    //
//    // Summary:
//    //     A special key masking the real key being processed by an IME.
//    ImeProcessed = 155,
//    //
//    // Summary:
//    //     A special key masking the real key being processed as a system key.
//    System = 156,
//    //
//    // Summary:
//    //     The OEM ATTN key.
//    OemAttn = 157,
//    //
//    // Summary:
//    //     The DBE_ALPHANUMERIC key.
//    DbeAlphanumeric = 157,
//    //
//    // Summary:
//    //     The OEM FINISH key.
//    OemFinish = 158,
//    //
//    // Summary:
//    //     The DBE_KATAKANA key.
//    DbeKatakana = 158,
//    //
//    // Summary:
//    //     The OEM COPY key.
//    OemCopy = 159,
//    //
//    // Summary:
//    //     The DBE_HIRAGANA key.
//    DbeHiragana = 159,
//    //
//    // Summary:
//    //     The OEM AUTO key.
//    OemAuto = 160,
//    //
//    // Summary:
//    //     The DBE_SBCSCHAR key.
//    DbeSbcsChar = 160,
//    //
//    // Summary:
//    //     The OEM ENLW key.
//    OemEnlw = 161,
//    //
//    // Summary:
//    //     The DBE_DBCSCHAR key.
//    DbeDbcsChar = 161,
//    //
//    // Summary:
//    //     The OEM BACKTAB key.
//    OemBackTab = 162,
//    //
//    // Summary:
//    //     The DBE_ROMAN key.
//    DbeRoman = 162,
//    //
//    // Summary:
//    //     The ATTN key.
//    Attn = 163,
//    //
//    // Summary:
//    //     The DBE_NOROMAN key.
//    DbeNoRoman = 163,
//    //
//    // Summary:
//    //     The CRSEL key.
//    CrSel = 164,
//    //
//    // Summary:
//    //     The DBE_ENTERWORDREGISTERMODE key.
//    DbeEnterWordRegisterMode = 164,
//    //
//    // Summary:
//    //     The EXSEL key.
//    ExSel = 165,
//    //
//    // Summary:
//    //     The DBE_ENTERIMECONFIGMODE key.
//    DbeEnterImeConfigureMode = 165,
//    //
//    // Summary:
//    //     The ERASE EOF key.
//    EraseEof = 166,
//    //
//    // Summary:
//    //     The DBE_FLUSHSTRING key.
//    DbeFlushString = 166,
//    //
//    // Summary:
//    //     The PLAY key.
//    Play = 167,
//    //
//    // Summary:
//    //     The DBE_CODEINPUT key.
//    DbeCodeInput = 167,
//    //
//    // Summary:
//    //     The ZOOM key.
//    Zoom = 168,
//    //
//    // Summary:
//    //     The DBE_NOCODEINPUT key.
//    DbeNoCodeInput = 168,
//    //
//    // Summary:
//    //     A constant reserved for future use.
//    NoName = 169,
//    //
//    // Summary:
//    //     The DBE_DETERMINESTRING key.
//    DbeDetermineString = 169,
//    //
//    // Summary:
//    //     The PA1 key.
//    Pa1 = 170,
//    //
//    // Summary:
//    //     The DBE_ENTERDLGCONVERSIONMODE key.
//    DbeEnterDialogConversionMode = 170,
//    //
//    // Summary:
//    //     The OEM Clear key.
//    OemClear = 171,
//    /// <summary>
//    /// The key is used with another key to create a single combined character.
//    /// </summary>
//    DeadCharProcessed = 172,
//    /// <summary>
//    /// The left mouse button.
//    /// </summary>
//    LeftMouseButton,
//    /// <summary>
//    /// The middle mouse button.
//    /// </summary>
//    MiddleMouseButton,
//    /// <summary>
//    /// The right mouse button.
//    /// </summary>
//    RightMouseButton,
//    /// <summary>
//    /// The first extended mouse button.
//    /// </summary>
//    ThumbMouseButton1,
//    /// <summary>
//    /// The second extended mouse button.
//    /// </summary>
//    ThumbMouseButton2,
//    /// <summary>
//    /// When the mouse Scrolls Down
//    /// </summary>
//    MouseScrollDown,
//    /// <summary>
//    /// When the mouse Scrolls Up
//    /// </summary>
//    MouseScrollUp
//}