//using Godot;
//using System.Collections.Generic;

//namespace dot_battle_signal_bus.scripts
//{
//    public partial class TurnManager : Node
//    {
//        public static TurnManager Instance { get; private set; }

//        private List<player> _players = new List<player>();
//        private int _currentPlayerIndex = 0;

//        public override void _Ready()
//        {
//            base._Ready();
//            Instance = this;
//            _players = new List<player>();
//            _currentPlayerIndex = 0;
//        }


//        public void AddPlayer(player newPlayer)
//        {
//            _players.Add(newPlayer);
//        }

//        public player GetCurrentPlayer()
//        {
//            if (_players.Count == 0) return null;
//            return _players[_currentPlayerIndex];
//        }

//        public void NextTurn()
//        {
//            if (_players.Count == 0) return;
//            _currentPlayerIndex = (_currentPlayerIndex + 1) % _players.Count;
//        }
//    }
//}
