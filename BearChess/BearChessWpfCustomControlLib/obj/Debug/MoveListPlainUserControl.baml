   M S B A M L   `   `   `  ����W  SBearChessWpfCustomControlLib, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null�Hclr-namespace:www.SoLaNoSoft.com.BearChessWpfCustomControlLib.Properties:www.SoLaNoSoft.com.BearChessWpfCustomControlLib.Properties  5      q=clr-namespace:www.SoLaNoSoft.com.BearChessWpfCustomControlLib/www.SoLaNoSoft.com.BearChessWpfCustomControlLib  5      N    Hwww.SoLaNoSoft.com.BearChessWpfCustomControlLib.MoveListPlainUserControl   5      W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      VpHclr-namespace:www.SoLaNoSoft.com.BearChessWpfCustomControlLib.Properties     5      Olocal=clr-namespace:www.SoLaNoSoft.com.BearChessWpfCustomControlLib     5      $��12(�5   a     J� CommandBindings  5
      )  #System.Windows.Input.CommandBinding  5   
   -   5           Command$
 Copy��5      6s   .��5      �  � RowDefinitions 5   
   �� 5      $
��Auto �6   6,   �� 5      $��* �6   6)   �� 5      $
��Auto �6   6,   �� 5      $
��Auto �6   6,   .+�5   
   � $��5q�6   $��0��6    5      �� 5      $
��Auto �6    60   �� 5      $
��Auto �6    60   �� 5      $
��Auto �6    60    � ColumnDefinitions 5      �� 5      $
��Auto �6#   62   �� 5      $
��Auto �6#   62   �� 5      $
��Auto �6#   62   �� 5      $
��Auto �6#   62   .+�5       �� -   5        $��textBlockResult��5    9   $��5q�6   $��DemiBold"�6#   $��0��6P   $��2��6`   $��1��6u   *6�   �� 5!      -   5        $��textBlockWhite��5!      $��Center=�6.   $��5q�6I   $��0��6T   $��0��6a   $��16(�6q   K6�   �� 5"      -   5        $��textBlockWhitePlayer��5"   F   $��2,5,5,5q�6   $��0��6)   $��1��66   $��16(�6b   J   Dwww.SoLaNoSoft.com.BearChessWpfCustomControlLib.Properties.Resources   White#��Z 6p   �� 5#      -   5        $��textBlockBlack��5#      $��Center=�6.   $��5q�6J   $��0��6U   $��2��6b   $��16(�6r   k6�   �� 5$      -   5        $��textBlockBlackPlayer��5$   F   $��2,5,5,0q�6   $��0��6)   $��3��66   $��16(�6b      Black#��Z 6p   5%      �� 5'   
   -   5        $��scrollViewer��5'      $��1��6+   $��Disabled=�68   .��5(      �� -   5        $��stackPanelMoves��5(      60   5)      ]� 5*   
   $��5q�6   $��2��6   ��.  .+�5,      �� -	   5        $��buttonExtend��5,       $��5q�6   $��20��6Q   $��20��6\   
������ 
������ $
��Left=�5-         ChangeContent � ToolTip# Z 6+   .��5.      �� $%��../Assets/Icons/script_gear.png��6   5/      �� 50      -
   5        $��buttonShowHideComments��50       $��5q�6   $��20��6e   $��20��6p   
������ 
������ $
��Left=�51         ShowHideComments# Z 6+   .��52      �� $"��../Assets/Icons/comments.png��6   53      �� 54      -   5        $��buttonShowHideBuddy��54       $��5q�6   $��20��6_   $��20��6j   
������ 
������ $
��Left=�55      	   ShowHideBuddyComments# Z	 60   .��56      �� $&�� ../Assets/Icons/user_comment.png��6   57      �� 58      -   5        $��buttonFontInc��58       $��5q�6   $��20��6S   $��20��6^   
������ 
������ $
��Left=�59      
   IncreaseFontSize# Z
 6+   .��5:      �� $,��&../Assets/Icons/font_size_decrease.png��6   5;      �� 5<      -   5        $��buttonFontDec��5<       $��5q�6   $��20��6S   $��20��6^   
������ 
������ $
��Left=�5=         DecreaseFontSize# Z 6+   .��5>      �� $,��&../Assets/Icons/font_size_increase.png��6   5?      �� 5@      -   5        $��
buttonCopy��5@   =   $��15,5,5,5q�6   $
��Left=�6'   $
��Copy��6Q   
������ 
������    CopyToClipboard# Z 6�   .��5A      �� $,��&../Assets/Icons/clipboard_sign_out.png��6   $��20��6H   $��20��6S   6a   5B      5D      �� 5E   
   -   5         ��Name$ textBlockContent��5E      $��5q�6.   $��3��69   $y�	Collapsed=�6F   Content: Only moves6s   5F      5G      