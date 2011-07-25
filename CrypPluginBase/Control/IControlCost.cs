﻿/*
   Copyright 2009 Thomas Schmid

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cryptool.PluginBase.Control
{

  /// <summary>
  /// Which operator has to be used to relate two values of the cost function
  /// </summary>
  public enum RelationOperator
  {
      LessThan, LargerThan
  }

  public interface IControlCost : IControl
  {   
      /// <summary>
      /// Returns the relation operator which has to be used to relate two values of the cost function
      /// </summary>
      /// <returns>RelationOperator</returns>
      RelationOperator GetRelationOperator();
      
      /// <summary>
      /// Calculate a value for the given text
      /// </summary>
      /// <param name="text">Text to calculate</param>
      /// <returns>cost</returns>
      double CalculateCost(byte[] text);

      /// <summary>
      /// This method modifies the given OpenCL code, so that the returning code includes
      /// the costfunction calculations.
      /// </summary>
      /// <param name="code"></param>
      /// <returns></returns>
      string ModifyOpenCLCode(string code);

      int GetBytesToUse();

      int GetBytesOffset();
  }
}
