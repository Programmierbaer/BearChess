   M S B A M L   `   `   `  ����G  CBearChessWin, Version=1.2.0.0, Culture=neutral, PublicKeyToken=nullk:clr-namespace:www.SoLaNoSoft.com.BearChessWin.UserControls,www.SoLaNoSoft.com.BearChessWin.UserControls  5      9    3www.SoLaNoSoft.com.BearChessWin.FicsGameUserControl   5      W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      Llocal:clr-namespace:www.SoLaNoSoft.com.BearChessWin.UserControls     5      .��5	      �   � ColumnDefinitions  5
      �� 5      $
��Auto �6   6+    � RowDefinitions 5   
   �� 5      $
��Auto �6   6,   �� 5      $
��Auto �6   6,   �� 5      $
��Auto �6   6,   �� 5      $
��Auto �6   6,   �� 5      $
��Auto �6   6,   .+�5   
   �� $��0q�6   $��0��6    $��0��60    �� Orientation$ 
Horizontal=�6=   $��2��6V   .+�5      �� $��SemiBold"�6   $��5q�6*   .��65   Info:6<   �� 5      -   5        $��textBlockInformation��5   !   $	��0,5q�6   .��6=   	No game6F   5      �� 5   
   $��0q�6   $��0��6    $��1��60   $ 
Horizontal=�6=   .+�5      �� $��SemiBold"�6   $��50��6*   $��5q�65   .��6@   White:6H   �� 5      -   5        $��textBlockPlayerWhite��5   !   $	��0,5q�6   6?   �� 5      $��5q�6   $��SemiBold"�6   .��66   Elo:6<   �� 5      -   5        $��textBlockPlayerWhiteElo��5   !   $	��0,5q�6   6B   5      �� 5   
   $��0q�6   $��2��6!   $ 
Horizontal=�6.   .+�5      �� $��SemiBold"�6   $��5q�6*   $��50��65   .��6A   Black:6I   �� 5       -   5        $��textBlockPlayerBlack��5    !   $	��0,5q�6   6?   �� 5!      $��5q�6   $��SemiBold"�6   .��66   Elo:6<   �� 5"      -   5        $��textBlockPlayerBlackElo��5"   !   $	��0,5q�6   6B   5#      �� 5$   
   $��0q�6   $��0��6    $��3��60   $ 
Horizontal=�6=   .+�5%      �� $��SemiBold"�6   $��5q�6*   .��65   Time control:6D   �� 5&      -   5        $��textBlockTimeControl��5&   !   $	��0,5q�6   6?   5'      �� 5(   
   $��0q�6   $��0��6    $��4��60   $ 
Horizontal=�6=   .+�5)      �� $��5q�6   $��SemiBold"�6   .��66   Say:6<   �� 5*      -   5        $��textBoxOpponentMessage��5*   !   $��5q�6   $	��300��6[   6i   �� 5+      -   5        $��buttonCommonSend��5+      $��Center=�6-   $��0,0,5,0q�6H   $��24��6Y   $��24��6d   
������ 
������  � ToolTip$ Say to your opponent��5,      .��5-      �� $#��../Assets/Icons/bullet_go.png��6   6A   5.      5/      50      51      