   M S B A M L   `   `   `  ����W  SBearChessWpfCustomControlLib, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullq=clr-namespace:www.SoLaNoSoft.com.BearChessWpfCustomControlLib/www.SoLaNoSoft.com.BearChessWpfCustomControlLib  5      �Hclr-namespace:www.SoLaNoSoft.com.BearChessWpfCustomControlLib.Properties:www.SoLaNoSoft.com.BearChessWpfCustomControlLib.Properties  5      J    Dwww.SoLaNoSoft.com.BearChessWpfCustomControlLib.MovePlainUserControl   5      W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      Olocal=clr-namespace:www.SoLaNoSoft.com.BearChessWpfCustomControlLib     5      VpHclr-namespace:www.SoLaNoSoft.com.BearChessWpfCustomControlLib.Properties     5      $��18(�5      .��5
      �� -   5        $��stackPanelMove��5
      .+�5   
   �   � Orientation$  
Horizontal=�6   .+�5      �� -   5        $��button��5   O   $	��Top=�6   
������ $
��Left=�5       � ContextMenu 5      �� 5      .@�5      m� J   Dwww.SoLaNoSoft.com.BearChessWpfCustomControlLib.Properties.Resources   
MoveSymbol#��Z 6#   .@�5      m� -   5         m� Icon$ !��5   '      GoodMove#��Z 60   6Y   m� 5      -   5        $ ?��5   '      PoorMove#��Z 60   6Y   m� 5      -   5        $ !!��5   '      VeryGoodMove#��Z 61   6^   m� 5      -   5        $ ??��5   '      VeryPoorMove#��Z 61   6^   m� 5      -   5        $ !?��5   '      SpeculativeMove#��Z 61   6a   m� 5      -   5        $ ?!��5   '   	   QuestionableMove#��Z	 61   6b   m� 5      -	   5        $	 □��5   '   
   OnlyMove#��Z
 60   6Y   �� 5      m� 5      -
   5           RemoveSymbol#��Z 5   '    5   "   �� 5   &   $��../Assets/Icons/bin.png��6,   5      5      m� 5         EvaluationSymbol#��Z 6#   .@�5       m� -   5        $ +-��5    '       WhiteHasDecisiveAdvantage#��Z 61   5!   %   m� 5"      -   5        $ -+��5"   '   #��Z 61   5#   %   m� 5$      -   5        $ =��5$   '      EqualPosition#��Z 60   6^   m� 5%      -   5        $	 ∞��5%   '      UnclearPosition#��Z 60   6`   m� 5&      -   5        $ +=��5&   '      WhiteSlightAdvantage#��Z 61   5'   %   m� 5(      -   5        $ =+��5(   '      BlackSlightAdvantage#��Z 61   5)   %   m� 5*      -   5        $ ±��5*   '      WhiteModerateAdvantage#��Z 60   5+   %   m� 5,      -   5        $	 ∓��5,   '      BlackModerateAdvantage#��Z 60   5-   %   m� 5.      -   5        $
 ∞=��5.   '      WithCompensation#��Z 61   6b   �� 5/      m� 50      -   5        #��Z 50   '    51   "   �� 52   &   $��../Assets/Icons/bin.png��6,   54      55      m� 56      -   5        $��menuItemEditSymbol��56   M      	OwnSymbol#��Z 6#    58      �� 59   "   $*��$../Assets/Icons/highlighter_text.png��6(   5;      m� 5<      -   5        $��menuItemEditComment��5<   N      
EditComent#��Z 6#    5>      �� 5?   "   $$��../Assets/Icons/script_add.png��6(   5A      m� 5B      -   5           RestartFromHere#��Z 5B   #    5C      �� 5D   "   $-��'../Assets/Icons/control_repeat_blue.png��6(   5F      5G      .��5I      � $  
Horizontal=�6   .+�5J      �� -   5        $��textBlockMoveNumber��5K      $��0,1,1,1q�5J       $��Calibri)�61   $��Normal&�6F   $��DemiBold"�6[   $��Center=�5L       �� TextAlignment$
 Left=�69   $
��Left=�6N   99.5N      �� 5O      -   5        $��textBlockFigureSymbol��5O   [   $��0,1,0,1q�6    $��Calibri)�61   $��Normal&�6F   $��Center=�5P      $
 Left=�69   $
��Left=�6N   Q5R      �� 5S      -   5        $��textBlockMove��5S   [   $��0,1,0,1q�6    $��Calibri)�61   $��Normal&�6F   $��Center=�5T      $
 Left=�69   $
��Left=�6N   e2-e45V      �� 5X      -   5        $��textBlockPromotionSymbol��5X   [   $��0,1,0,1q�6    $��Calibri)�61   $��Normal&�6F   $y�	Collapsed=�5Y      $��Center=�65   $
 Left=�6P   $
��Left=�6e   Q5\      �� 5]      -   5        $��textBlockCheckMateSymbol��5]   [   $��0,1,0,1q�6    $��Calibri)�61   $��Normal&�6F   $y�	Collapsed=�5^      $��Center=�65   $
 Left=�6P   $
��Left=�6e   +5a      �� 5b      -   5        $��textBlockMoveSymbol��5b   [   $��0,1,0,1q�6    $��Calibri)�61   $��Normal&�6F   $y�	Collapsed=�5c      $��DemiBold"�65   $��Center=�5d      $
 Left=�69   $
��Left=�6N   
���  ��!5g      �� 5j      -   5        $��textBlockMoveEvaluation��5k      $��0,1,0,1q�5j       $��Calibri)�61   $��Normal&�6F   $y�	Collapsed=�5k   =   $��DemiBold"�6T   $��Center=�5l      $
 Left=�69   $
��Left=�6N   
���  ��!5o      �� 5p      -   5        $��textBlockOwnSymbol��5q      $��0,1,0,1q�5p       $��Calibri)�61   $��Normal&�6F   $y�	Collapsed=�5q   8   $��DemiBold"�6O   $��Center=�5r      $
 Left=�69   $
��Left=�6N   
���  ��!5u      �� 5v      -    5        $��textBlockCommentInternal��5w      $��2,1,0,1q�5v       $��Calibri)�61   $��Normal&�6F   
��� � �$
��Wrap=�5y      $	��Top=�62   $
 Left=�6J   $y�	Collapsed=�6_   $
��Left=�5z      	Comment5|      5}      ��5~      �� 5       �� 
TargetType" ��6&   .R�5�      �� $��Border��6!   #��z��6/   #��z��5�      #��z��5�      ._�5�      �� #��z��6/   #��z��5�   -   #��z��5�   -   5�       �� Triggers 5�      P� 5�       P� Property �@J�	IsFocused P� Value . . �5�   "   ��  �� 
TargetName$ Border��6)    �� Property � �� �� Value
 ��ح�6S   5�      P� 5�       �@J�	IsFocused .  . �5�   "   �� $ Border��6)    � ��
 ���� 6S   5�      5�      5�      5�      �� 5�   
   -!   5        $��textBlockComment��5�   P   $��25,2,1,1q�6   $��Calibri)�6&   $��Normal&�6;   
��� � �$
��Wrap=�5�      $	��Top=�6&   $
 Left=�6>   $y�Visible=�6S   $
��Left=�5�      	Comment5�      � 5�   
   -"   5        $��WrapPanelBestLine��5�   -   $  
Horizontal=�6   .+�5�      �� -#   5        $��textBlockMoveValue��5�   T   $��25,2,2,1q�6   $��Calibri)�6*   $��Normal&�6?   $
��Wrap=�5�      $	��Top=�6*   $
 Left=�6B   $
��Left=�5�      15�      �� 5�      -$   5        $��textBlockBestLine��5�   S   $��3,2,1,1q�6   $��Calibri)�6)   $��Normal&�6>   
����  �$
��Wrap=�5�      $	��Top=�6*   $
 Left=�6B   $
��Left=�5�      	Comment5�      5�      � 5�   
   -%   5        $��WrapPanelBestLineBuddy��5�   -   $  
Horizontal=�6   .+�5�      �� -&   5        $��textBlockMoveValueBuddy��5�   T   $��25,2,2,1q�6   $��Calibri)�6*   $��Normal&�6?   $
��Wrap=�5�      $	��Top=�6,   $
 Left=�6D   $
��Left=�5�      15�      �� 5�      -'   5        $��textBlockBestLineBuddy��5�   S   $��3,2,1,1q�6   $��Calibri)�6)   $��Normal&�6>   
����2��$
��Wrap=�5�      $	��Top=�6,   $
 Left=�6D   $
��Left=�5�      	Comment5�      5�      5�      5�      