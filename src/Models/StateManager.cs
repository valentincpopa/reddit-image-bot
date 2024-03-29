﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace RedditImageBot.Models
{
    public abstract class StateManager<T>
    {
        public StateManager()
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("The type of the internal state should be an enum.");
            }

            StateTransitions = new();
        }

        public T CurrentState { get; private set; }

        private List<StateTransition<T>> StateTransitions { get; }

        public void AddTransition(T source, T destination)
        {
            var stateTransition = new StateTransition<T>(source, destination);
            StateTransitions.Add(stateTransition);
        }

        public void SetCurrentState(T currentState)
        {
            CurrentState = currentState;
        }

        public void ChangeState(T destination)
        {
            if (IsValidTransition(CurrentState, destination))
            {
                CurrentState = destination;
                return;
            }

            throw new InvalidOperationException($"No valid transition from: '{CurrentState}' to '{destination}' exists.");
        }

        private bool IsValidTransition(T source, T destination)
        {
            return StateTransitions.Any(x =>
                       x.Source.Equals(source)
                    && x.Destination.Equals(destination));
        }
    }
}
