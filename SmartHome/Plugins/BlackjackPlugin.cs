using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace SmartHome.Plugins
{
    public class BlackjackPlugin
    {
        private List<string> _deck;
        private List<string> _playerHand;
        private List<string> _dealerHand;
        private bool _gameInProgress;

        public BlackjackPlugin()
        {
            ResetGame();
        }

        private void ResetGame()
        {
            _deck = new List<string>();
            // Using symbols as requested: Hearts -> ♥, Diamonds -> ♦, Clubs -> ♣, Spades -> ♠
            string[] suits = { "♥", "♦", "♣", "♠" };
            string[] ranks = { "2", "3", "4", "5", "6", "7", "8", "9", "10", "Jack", "Queen", "King", "Ace" };

            foreach (var suit in suits)
            {
                foreach (var rank in ranks)
                {
                    // Format: "Rank Suit" (e.g., "Ace ♠")
                    _deck.Add($"{rank} {suit}");
                }
            }

            ShuffleDeck();
            _playerHand = new List<string>();
            _dealerHand = new List<string>();
            _gameInProgress = false;
        }

        private void ShuffleDeck()
        {
            Random rng = new Random();
            int n = _deck.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                string value = _deck[k];
                _deck[k] = _deck[n];
                _deck[n] = value;
            }
        }

        private int CalculateScore(List<string> hand)
        {
            int score = 0;
            int aces = 0;

            foreach (var card in hand)
            {
                // Split by space to separate Rank and Suit symbol
                var parts = card.Split(' ');
                var rank = parts[0];

                if (int.TryParse(rank, out int value))
                {
                    score += value;
                }
                else if (rank == "Jack" || rank == "Queen" || rank == "King")
                {
                    score += 10;
                }
                else if (rank == "Ace")
                {
                    aces++;
                    score += 11;
                }
            }

            while (score > 21 && aces > 0)
            {
                score -= 10;
                aces--;
            }

            return score;
        }

        [KernelFunction("new_hand")]
        [Description("Starts a new hand of Blackjack. Resets the game and deals initial cards.")]
        public string NewHand()
        {
            ResetGame();
            _gameInProgress = true;

            // Deal 2 cards to player
            _playerHand.Add(DrawCard());
            _playerHand.Add(DrawCard());

            // Deal 2 cards to dealer
            _dealerHand.Add(DrawCard());
            _dealerHand.Add(DrawCard());

            int playerScore = CalculateScore(_playerHand);

            return $"Game started. Your hand: {string.Join(", ", _playerHand)} (Score: {playerScore}). Dealer shows: {_dealerHand[0]} and [Hidden Card].";
        }

        [KernelFunction("hit")]
        [Description("Player requests another card.")]
        public string Hit()
        {
            if (!_gameInProgress)
            {
                return "No game in progress. Please say 'new hand' to start.";
            }

            string newCard = DrawCard();
            _playerHand.Add(newCard);
            int score = CalculateScore(_playerHand);

            if (score > 21)
            {
                _gameInProgress = false;
                return $"You drew {newCard}. Your hand: {string.Join(", ", _playerHand)} (Score: {score}). BUST! You lose.";
            }

            return $"You drew {newCard}. Your hand: {string.Join(", ", _playerHand)} (Score: {score}). Dealer shows: {_dealerHand[0]}. Hit or Stand?";
        }

        [KernelFunction("stand")]
        [Description("Player holds their hand. Dealer plays.")]
        public string Stand()
        {
            if (!_gameInProgress)
            {
                return "No game in progress. Please say 'new hand' to start.";
            }

            int playerScore = CalculateScore(_playerHand);
            int dealerScore = CalculateScore(_dealerHand);

            // Dealer draws until 17 or higher
            while (dealerScore < 17)
            {
                _dealerHand.Add(DrawCard());
                dealerScore = CalculateScore(_dealerHand);
            }

            _gameInProgress = false;
            string dealerCards = string.Join(", ", _dealerHand);
            string result;

            if (dealerScore > 21)
            {
                result = "Dealer busts! You win!";
            }
            else if (dealerScore > playerScore)
            {
                result = "Dealer wins.";
            }
            else if (dealerScore < playerScore)
            {
                result = "You win!";
            }
            else
            {
                result = "It's a tie (Push).";
            }

            return $"You stood at {playerScore}. Dealer has: {dealerCards} (Score: {dealerScore}). {result}";
        }

        private string DrawCard()
        {
            if (_deck.Count == 0)
            {
                return "No cards left";
            }
            var card = _deck[0];
            _deck.RemoveAt(0);
            return card;
        }
    }
}
