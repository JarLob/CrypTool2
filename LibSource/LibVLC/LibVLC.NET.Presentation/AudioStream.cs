﻿////////////////////////////////////////////////////////////////////////////////
//
//  Presentation/AudioStream.cs - This file is part of LibVLC.NET.
//
//    Copyright (C) 2011 Boris Richter <himself@boris-richter.net>
//
//  ==========================================================================
//  
//  LibVLC.NET is free software; you can redistribute it and/or modify it 
//  under the terms of the GNU Lesser General Public License as published by 
//  the Free Software Foundation; either version 2.1 of the License, or (at 
//  your option) any later version.
//    
//  LibVLC.NET is distributed in the hope that it will be useful, but WITHOUT 
//  ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or 
//  FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public 
//  License for more details.
//
//  You should have received a copy of the GNU Lesser General Public License 
//  along with LibVLC.NET; if not, see http://www.gnu.org/licenses/.
//
//  ==========================================================================
// 
//  $LastChangedRevision$
//  $LastChangedDate$
//  $LastChangedBy$
//
////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibVLC.NET.Presentation
{

  //****************************************************************************
  /// <summary>
  ///   Represents an audio stream of a media loaded within a media element.
  /// </summary>
  /// <seealso cref="MediaElement"/>
  public sealed class AudioStream
    : MediaStream
  {

    //==========================================================================
    internal AudioStream(AudioTrack audioTrack)
      : base(audioTrack)
    { 
      // ...
    }

    #region Properties

    #region AudioTrack

    //==========================================================================                
    internal AudioTrack AudioTrack
    {
      get
      {
        return Track as AudioTrack;
      }
    }

    #endregion // AudioTrack

    #region Channels

    //==========================================================================                
    /// <summary>
    ///   Gets the number of channels of the audio track.
    /// </summary>
    public int Channels
    {
      get
      {
        return AudioTrack.Channels;
      }
    }

    #endregion // Channels

    #region BitRate

    //==========================================================================                
    /// <summary>
    ///   Gets bit rate of the audio track.
    /// </summary>
    public int BitRate
    {
      get
      {
        return AudioTrack.BitRate;
      }
    }

    #endregion // BitRate

    #endregion // Properties



  } // class AudioStream

}
