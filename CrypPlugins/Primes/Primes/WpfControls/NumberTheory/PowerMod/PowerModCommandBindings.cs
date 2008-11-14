﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Primes.WpfControls.NumberTheory.PowerMod
{
  public partial class PowerModControl
  {
    private void initBindings()
    {
      
      this.CommandBindings.Add(
        new CommandBinding(
          PowerModCommands.ReOrderPointsCommand,  
          new ExecutedRoutedEventHandler(ReOrderPoints), 
          new CanExecuteRoutedEventHandler(CanReOrderPoints)));
    }
  }
}
