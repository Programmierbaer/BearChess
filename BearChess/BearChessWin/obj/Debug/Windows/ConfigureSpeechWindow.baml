   M S B A M L   `   `   `  ����G  CBearChessWin, Version=1.2.0.0, Culture=neutral, PublicKeyToken=nullg8clr-namespace:www.SoLaNoSoft.com.BearChessWin.Properties*www.SoLaNoSoft.com.BearChessWin.Properties  5   	   ;    5www.SoLaNoSoft.com.BearChessWin.ConfigureSpeechWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   Fp8clr-namespace:www.SoLaNoSoft.com.BearChessWin.Properties     5   	     � WindowStartupLocation$  CenterOwner=�5       � 
ResizeMode$ NoResize=�6>   :   4www.SoLaNoSoft.com.BearChessWin.Properties.Resources   SpeechSoundConfiguration � Title# Z 5   	   $	��680��6A   $	��500��6N    � SizeToContent$ Height=�6Z   .��5	      �  � RowDefinitions 5
   
   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $	��120 �6   �� 5      $
��Auto �6   �� 5      $	��320 �6   �� 5      $
��Auto �6   .+�5   
   �� $��0��6   $��5q�6"   .+�5      ��  �� Orientation$ 
Horizontal=�6   .+�5      �� -   5        $��checkBoxSpeechActive��5   H   $��5q�5      $��DemiBold"�6O      EitherVoiceSound � ToolTip# Z 6e   	   SpeechActive#��Z	 5   d   �� 5      -   5        $��textBlockSpeechAvailable��5      $y�Hidden=�6=   $��10,5,5,5q�6Q   $��DemiBold"�6c   
���  ��$��Center=�6�   
   SpeechNotAvailable#��Z
 6�   5      �� 5      $ 
Horizontal=�6   .+�5      �� $��15,5,5,5q�6   $��Center=�6.    �� TextAlignment$ Center=�6I      TypeMoveAnnouncement#��Z 6`   	� 5      -   5        $��radioButtonLong��5   D    	� 	GroupName$ moveType��6   $��5,5,5,5q�63    d� 	IsChecked$
 TrueY�6[      
KnightG1F3#��Z 6m   	� 5      -   5        $��radioButtonShort��5   >   $ moveType��6   $��5q�63      KnightF3#��Z 6V   5      �� 5!      -   5        $��checkBoxOwnMove��5!      $��15,5,5,5q�6.      AnnounceOwnMove# Z 6@      OwnMove#��Z 6q   5"      �� 5#   
   $
��Left=�6   $��DemiBold"�6*   $��15,5,5,4q�6@   $��1��6R      Language#��Z 6`   &   System.Windows.Controls.DataGrid  5%   
   -   5        $��dataGridSpeech��5%      $��15,5,5,5q�6A      AutoGenerateColumns .  $��2��5&         SelectLanguage# Z 6       SelectionMode$ Extended=�5'      "   VerticalScrollBarVisibility$
 Auto=�5(         
IsReadOnly . ��5%   )   ��  � 	Resources 5*       �� Color��%�   (��       5+   "   *1/SystemColors.InactiveSelectionHighlightBrushKey+)(�� ,     5-   "   *53SystemColors.InactiveSelectionHighlightTextBrushKey+)�� 5+      $ CornflowerBlue��5,   "   �� 5-      $ White��5.   "   5/         Columns 51      0  *System.Windows.Controls.DataGridTextColumn  52      ,  &System.Windows.Controls.DataGridColumn   Width5  /System.Windows.Controls.DataGridLengthConverter$
 Auto 6H      FontSize$ 13Q�6U   1  +System.Windows.Controls.DataGridBoundColumn   Binding 6%   �� *VoiceInfo.Name+   Header 53      �� 54          Name#��Z  6$   6I   56        58      $
 Auto 6W   $ 13Q�59   %    58   %   �� *VoiceInfo.Culture.DisplayName+ 5:      �� 5;      !   CultureName#��Z! 6$   6P   5=        5>      $
 Auto 6J   $ 13Q�6W    6%   �� *VoiceInfo.Gender+ 5?      �� 5@      "   Gender#��Z" 6$   6K   5B        5C      $
 Auto 6G   $ 13Q�6T    6%   �� *VoiceInfo.Age+ 5D      �� 5E      
#   Age#��Z# 6$   6H   5G      5K      �� 5L   
   $��3��6   $ 
Horizontal=�6#   .+�5M      �� -   5        $��checkBoxOwnLanguage��5M      $��15,5,5,5q�6�   $   DefineOwnTranslation#��Z$ 6�   �� 5N      -   5        $��5q�5N      $��DemiBold"�6    $��28��66   $��28��6A   
������ 
������ .��5O      �� $ 
Horizontal=�6   .+�5P      �� $$��../Assets/Icons/world_edit.png��6   6F   5Q      5R      5S      �� 5T   
   -	   5        $��stackPanelSound��5T      $��4��6,   .+�5U      �� $��5q�6   
�������$��1q�63   ._�5V      � $��5q�6   % � ColumnDefinitions% 5W      �� 5X      $	��130 �6+   �� 5Y      $��* �6+   �� 5Z      $
��Auto �6+    5\      �� 5]      �� 5^      .+�5a      �� -
   5        $��checkBoxSoundMove��5a   G   $��0��6   $��0��6/   $��5q�6<   &   
EngineMove#��Z& 6`   �� 5b      -   5        $��buttonMovePlay��5b   :   $��2��6   $��0��6-   $��5q�6P   $��28��5c      $��28��6(   
������ 
������ ' �� 	IsDefault' . $
 Play��5d   <   .��5e      �� $��../Assets/Icons/sound.png��6    5f      �� 5g      $��0��6   $��1��6,   $��Center=�69   $��Right=�6[   (   	SoundFile#��Z( 6w   �� 5h      -   5        $��labelSoundMove��5h      $��1��62   $��1��6B   $��Center=�6O   �� 5i      $��2��6!   $��1��61   $ 
Horizontal=�6>   .+�5j      �� -   5        $��buttonMoveFile��5j   !   $��5,5,5,5q�67   $
��Left=�6H   $��32��5k   !   $��32��6,   
������ 
������ ' . )   SelectSoundFile# Z) 5m   @   .��5n      �� $,��&../Assets/Icons/file_extension_wav.png��6$   5o      �� 5p      -   5        $��buttonClearMoveFile��5p   !   $��5q�6<   $
��Left=�6G   $��28��5q   !   $��28��6,   
������ 
������ ' . *   ClearSoundFile# Z* 5s   E   .��5t      �� $ ��../Assets/Icons/delete.png��6$   5u      5v      5w      5x      �� 5y      $��5q�6   
�������$��1q�63   ._�5z      � $��5q�6   % 5{      �� 5|      $	��130 �6+   �� 5}      $��* �6+   �� 5~      $
��Auto �6+    5�      �� 5�      �� 5�      .+�5�      �� -   5        $��checkBoxSoundCheck��5�   G   $��0��6   $��0��6/   $��5q�6<   +   EngineCheck#��Z+ 6a   �� 5�      -   5        $��buttonCheckPlay��5�   :   $��2��6   $��0��6-   $��5q�6Q   $��28��5�      $��28��6(   
������ 
������ ' . $
 Play��5�   =   .��5�      �� $��../Assets/Icons/sound.png��6    5�      �� 5�      $��0��6   $��1��6,   $��Center=�69   $��Right=�6[   #��Z( 6w   �� 5�      -   5        $��labelSoundCheck��5�      $��1��63   $��1��6C   $��Center=�6P   �� 5�      $��2��6!   $��1��61   $ 
Horizontal=�6>   .+�5�      �� -   5        $��buttonCheckFile��5�   !   $��5,5,5,5q�68   $
��Left=�6I   $��32��5�   !   $��32��6,   
������ 
������ ' . # Z) 5�   A   .��5�      �� $,��&../Assets/Icons/file_extension_wav.png��6$   5�      �� 5�      -   5        $��buttonClearCheckFile��5�   !   $��5q�6=   $
��Left=�6H   $��28��5�   !   $��28��6,   
������ 
������ ' . # Z* 5�   F   .��5�      �� $ ��../Assets/Icons/delete.png��6$   5�      5�      5�      5�      �� 5�      $��5q�6   
�������$��1q�63   ._�5�      � $��5q�6   % 5�      �� 5�      $	��130 �6+   �� 5�      $��* �6+   �� 5�      $
��Auto �6+    5�      �� 5�      �� 5�      .+�5�      �� -   5        $��checkBoxSoundCheckMate��5�   G   $��0��6   $��0��6/   $��5q�6<   ,   
EngineMate#��Z, 6e   �� 5�      -   5        $��buttonCheckMatePlay��5�   :   $��2��6   $��0��6-   $��5q�6U   $��28��5�      $��28��6(   
������ 
������ ' . $
 Play��5�   A   .��5�      �� $��../Assets/Icons/sound.png��6    5�      �� 5�      $��0��6   $��1��6,   $��Center=�69   $��Right=�6[   #��Z( 6w   �� 5�      -   5        $��labelSoundCheckMate��5�      $��1��67   $��1��6G   $��Center=�6T   �� 5�      $��2��6!   $��1��61   $ 
Horizontal=�6>   .+�5�      �� -   5        $��buttonCheckMateFile��5�   !   $��5q�6<   $
��Left=�6G   $��32��5�   !   $��32��6,   
������ 
������ ' . # Z) 5�   E   .��5�      �� $,��&../Assets/Icons/file_extension_wav.png��6$   5�      �� 5�      -   5        $��buttonClearCheckMateFile��5�   !   $��5q�6A   $
��Left=�6L   $��28��5�   !   $��28��6,   
������ 
������ ' . # Z* 5�   J   .��5�      �� $ ��../Assets/Icons/delete.png��6$   5�      5�      5�      5�      5�      ]� 5�   
   $��5q�6   $��5��6   ��.  .+�5�      �� -   5        $��buttonOk��5�      $��5q�6%   $
��Left=�60   $��30��6F   $��30��6Q   
������ 
������ ' . .��5�      �� $'��!../Assets/Icons/accept_button.png��6   5�      �� 5�      -   5        $��buttonCancel��5�      $��5q�6)   $��Right=�64   $��30��6K   $��30��6V   
������ 
������ - �� IsCancel- . .��5�      �� $ ��../Assets/Icons/cancel.png��6   5�      5�      5�      5�      