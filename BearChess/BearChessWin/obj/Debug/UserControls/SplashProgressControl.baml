   M S B A M L   `   `   `  ����G  CBearChessWin, Version=1.2.0.0, Culture=neutral, PublicKeyToken=nullk:clr-namespace:www.SoLaNoSoft.com.BearChessWin.UserControls,www.SoLaNoSoft.com.BearChessWin.UserControls  5      ;    5www.SoLaNoSoft.com.BearChessWin.SplashProgressControl   5      W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      Llocal:clr-namespace:www.SoLaNoSoft.com.BearChessWin.UserControls     5      $
��Auto��5      $	��340��6   .��5	      � .+�5
   
   �� $��1,0,0,1q�6   $��1q�6"   
���   �._�5      �� .+�5      �� -   5          ��Name$  	TextLabel��5   ;   $
��Auto��6   $��5,5,4,0q�6*   $��Label��6N   
�������$��CharacterEllipsis=�5      $
��Wrap=�6=   �� 5      -   5        $  SubTextLabel��5   ;   $
��Auto��6   $��5,0,4,0q�6*   $��Sublabel��6Q   $��Italic$�6a   $y�Visible=�5      $��CharacterEllipsis=�61   � 5       � ColumnDefinitions 5      �� 5      $
��Auto �6+   �� 5      $
��Auto �6+    � RowDefinitions 5      �� 5      $��34 �6(   .+�5      � -   5         �Name$ ProgressBar��5   O   $	��260��6"   $��5,0,2,0q�6.   $��0��6?    � IsIndeterminate .  $��24��5   :   �� 5      -   5         ��Name$ CancelButton��5   >   $��5,0,4,0q�6   $��1��6.   $��32��6T   $��32��6_    � ToolTip$ Cancel game��6k   $y�Visible=�5      $��Right=�62   
������ 
������ .��5      �� $ ��../Assets/Icons/cancel.png��6    6F   5      5      �� 5      -   5        $  SubCancelLabel��5   -   $��5,0,4,0q�6   $��Cancel requested...��6E   
���  ��$��Italic$�5       $
��Bold"�6/   $y�Visible=�6A   $��CharacterEllipsis=�5!      5"      5#      5$      5%      