using BLREdit.UI;

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace BLREdit.API.Export;

public sealed class BLRServerProperties : INotifyPropertyChanged
{
    #region Events
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion Events

    #region PrivatFields
    [JsonIgnore] private float _GameRespawnTime = 10;
    [JsonIgnore] private float _GameForceRespawnTime = 30;
    [JsonIgnore] private float _GameSpectatorSwitchDelayTime = 120;
    [JsonIgnore] private int _NumEnemyVotesRequiredForKick = 4;
    [JsonIgnore] private int _NumFriendlyVotesRequiredForKick = 2;
    [JsonIgnore] private int _VoteKickBanSeconds = 1200;
    [JsonIgnore] private float _MaxIdleTime = 180;
    [JsonIgnore] private int _MinRequiredPlayersToStart = 1;
    [JsonIgnore] private float _PlayerSearchTime = 30;
    [JsonIgnore] private int _TimeLimit = 10;
    [JsonIgnore] private int _GoalScore = 3000;
    [JsonIgnore] private int _MaxBotCount;
    [JsonIgnore] private int _MaxPlayers = 16;
    [JsonIgnore] private int _NumBots;
    #endregion PrivateFields

    public ObservableCollection<string> RandomBotNames { get; set; } = [];
    public float GameRespawnTime { get { return _GameRespawnTime; } set { _GameRespawnTime = value; OnPropertyChanged(); } }
    public float GameForceRespawnTime { get { return _GameForceRespawnTime; } set { _GameForceRespawnTime = value; OnPropertyChanged(); } }
    public float GameSpectatorSwitchDelayTime { get { return _GameSpectatorSwitchDelayTime; } set { _GameSpectatorSwitchDelayTime = value; OnPropertyChanged(); } }
    public int NumEnemyVotesRequiredForKick { get { return _NumEnemyVotesRequiredForKick; } set { _NumEnemyVotesRequiredForKick = value; OnPropertyChanged(); } }
    public int NumFriendlyVotesRequiredForKick { get { return _NumFriendlyVotesRequiredForKick; } set { _NumFriendlyVotesRequiredForKick = value; OnPropertyChanged(); } }
    public int VoteKickBanSeconds { get { return _VoteKickBanSeconds; } set { _VoteKickBanSeconds = value; OnPropertyChanged(); } }
    public float MaxIdleTime { get { return _MaxIdleTime; } set { _MaxIdleTime = value; OnPropertyChanged(); } }
    public int MinRequiredPlayersToStart { get { return _MinRequiredPlayersToStart; } set { _MinRequiredPlayersToStart = value; OnPropertyChanged(); } }
    public float PlayerSearchTime { get { return _PlayerSearchTime; } set { _PlayerSearchTime = value; OnPropertyChanged(); } }
    public int TimeLimit { get { return _TimeLimit; } set { _TimeLimit = value; OnPropertyChanged(); } }
    public int GoalScore { get { return _GoalScore; } set { _GoalScore = value; OnPropertyChanged(); } }
    public UIBool KickOnIdleIntermission { get; set; } = new();
    public UIBool ForceBotsToSingleTeam { get; set; } = new();
    public UIBool AllowUnbalancedTeams { get; set; } = new();
    public int MaxBotCount { get { return _MaxBotCount; } set { _MaxBotCount = value; OnPropertyChanged(); } }
    public int MaxPlayers { get { return _MaxPlayers; } set { _MaxPlayers = value; OnPropertyChanged(); } }
    public int NumBots { get { return _NumBots; } set { _NumBots = value; OnPropertyChanged(); } }

    //TODO: Add BotProviders

}
