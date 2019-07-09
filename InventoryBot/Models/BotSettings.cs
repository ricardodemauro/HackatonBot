using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BertaBot.Models
{
    public class BotSettings
    {
        public string Name { get; set; }
        public string WelcomeMessage { get; set; }
        public string InternWarning { get; set; }
        public string WhatToDo { get; set; }
        public string AddCar { get; set; }
        public List<string> AddCarSynonyms { get; set; }
        public string SeeInventory { get; set; }
        public List<string> SeeInventorySynonyms { get; set; }
        public string Exit { get; set; }
        public List<string> ExitSynonyms { get; set; }
        public string AskForImages { get; set; }
        public string CarsReceived { get; set; }
        public string Processing { get; set; }
        public string ProcessingFinished { get; set; }
        public string Complain { get; set; }
        public string CheckAnswer { get; set; }
        public string CorrectAnwer { get; set; }
        public string WrongAnswer { get; set; }
        public string CorrectModel { get; set; }
    }
}
