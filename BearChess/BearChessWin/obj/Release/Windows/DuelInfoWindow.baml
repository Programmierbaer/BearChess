   M S B A M L   `   `   `  ����   BearChessWin4    .www.SoLaNoSoft.com.BearChessWin.DuelInfoWindow   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     -     � WindowStyle$  
ToolWindow=�
������� � Icon$' !../Assets/Icons/6-2-chess-png.png�� � Title$ Engine Duel Information��$	��220��$	��350��$	��220��$	��300��.���� .+�]� ��.  .+��� -   $��textBlockStatus��$
��Left=�$��5q�Current game 1 of 2�� -   $��checkBoxSwitchColor��$��Center=�$��Right=�{�.  $��0,0,50,0q��� $��80��$��Center=�$��Right=�.��Switch color:�� �� $��Visible=�$
��Auto=�.��� -   $��gridDuel�� � ColumnDefinitions �� $
��Auto ��� $��40 � � RowDefinitions �� �� �� .+�� $��0��$��0�� �� ��  �� �� �� $��5q�$��0��$
��Wrap=�$��0��$��DemiBold"�Engine / Game�� $��5q�$��0��$
��Wrap=�$��1��$��SemiBold"�$	��∑���� -   $��textBlockEngine1��$��5q�$��1��$��0��
Engine 1�� -   $��textBlockEngine2��$��5q�$��2��$��0��
Engine 2�� -   $��textBlockResult1��$��5q�$��1��$��1��$��SemiBold"�0�� -   $��textBlockResult2��$��5q�$��2��$��1��$��SemiBold"�0�� ]� $��5q�.+��� -	   $��buttonPause��$��Right=�$��5,5,10,5q�$��Right=�
������ 
������ .���� $+��%../Assets/Icons/control_stop_blue.png��$��32��$��32���� -
   $��buttonContinue��$��Right=�$��5,5,10,5q�$��Right=�$y�	Collapsed=�
������ 
������ .���� $+��%../Assets/Icons/control_play_blue.png��$��32��$��32���� -   $��buttonClose��$��Right=�$��5,5,10,5q�$��Right=�$y�	Collapsed=�
������ 
������ .���� $"��../Assets/Icons/door_out.png��$��32��$��32��