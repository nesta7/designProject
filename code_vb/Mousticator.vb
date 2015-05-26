Option Strict On
Option Explicit On

Public Class Mousticator

    Inherits HydroObject

    Private mStadeLarvaireIni As Single = 1.0F, mStadeLarvaire As Single = 1.0F
    Private mAutoriseDevIni As Short = 1, mAutoriseDev As Short = 1
    Private mDaysWithoutWaterIni As Single = 0.0F, mDaysWithoutWater As Single = 0.0F
    Private mNumberOfStepsPerDay As Single = 0.0F
    Private Model(4, 4) As Single

    'VARIABLES PARAMETERS INPUTS/OUTPUTS/RESULTS
    Private __z As String = "z"
    Private __TUp As String = "T"
    Private __Level As String = "Level"
    Private __StadeLarvaire As String = "StadeLarvaire"
    Private __AutoriseDev As String = "AutoriseDev"
    Private __DaysWithoutWater As String = "DaysWithoutWater"

    'PARAMETERS
    Private __z_parameter As Integer = 0
    Private __stadelarvaire_parameter As Integer = 1
    Private __AutoriseDev_parameter As Integer = 2
    Private __DaysWithoutWater_parameter As Integer = 3

    'INPUT/OUTPUT/RESULT
    Private __TUp_input As Integer = 0
    Private __Level_input As Integer = 1
    Private __StadeLarvaire_result As Integer = 0
    Private __AutoriseDev_result As Integer = 1
    Private __DaysWithoutWater_result As Integer = 2

    Public Sub New()

        'Fabrication de mes paramètres
        Dim P As Parameter
        MyBase.Type = ObjectsType.Mousticator

        'Mes paramètres
        '0
        P = New Parameter(ParameterTypeEnum.H, __z, "(m a.s.l.)")
        P.ParamValue = 677.0F
        MyBase.Parameters.Add(P)
        P = Nothing
        '1
        P = New Parameter(ParameterTypeEnum.NotDefined, __StadeLarvaire, "(-)")
        P.ParamValue = 1.0F
        MyBase.Parameters.Add(P)
        P = Nothing
        '2
        P = New Parameter(ParameterTypeEnum.NotDefined, __AutoriseDev, "(-)")
        P.ParamValue = 0.0F
        MyBase.Parameters.Add(P)
        P = Nothing
        '3
        P = New Parameter(ParameterTypeEnum.NotDefined, __DaysWithoutWater, "(-)")
        P.ParamValue = 0.0F
        MyBase.Parameters.Add(P)
        P = Nothing


        'mes Inputs
        '0
        P = New Parameter(ParameterTypeEnum.Temperature, __TUp, "(°C)")
        P.Position = ParameterPosition.Upstream
        MyBase.Inputs.Add(P)
        P = Nothing
        '1
        P = New Parameter(ParameterTypeEnum.H, __Level, "(m a.s.l.)")
        P.Position = ParameterPosition.Upstream
        MyBase.Inputs.Add(P)
        P = Nothing

        'mes Outputs

        'mes résultats
        '0
        MyBase.Results.Add(New Result(New Parameter(ParameterTypeEnum.NotDefined, __StadeLarvaire, "(-)")))
        '1
        MyBase.Results.Add(New Result(New Parameter(ParameterTypeEnum.NotDefined, __AutoriseDev, "(-)")))
        '2
        MyBase.Results.Add(New Result(New Parameter(ParameterTypeEnum.NotDefined, __DaysWithoutWater, "(-)")))

        MyBase.InitIntegralOutputs()
    End Sub

    Public Property z As Single
        Get
            Return CSng(MyBase.Parameters(__z_parameter).ParamValue)
        End Get
        Set(ByVal value As Single)
            MyBase.Parameters(__z_parameter).ParamValue = value
        End Set
    End Property

    Public Property StadeLarvaire As Single
        Get
            Return CSng(MyBase.Parameters(__stadelarvaire_parameter).ParamValue)
        End Get
        Set(ByVal value As Single)
            MyBase.Parameters(__stadelarvaire_parameter).ParamValue = value
        End Set
    End Property

    Public Property AutoriseDev As Integer
        Get
            Return CInt(MyBase.Parameters(__AutoriseDev_parameter).ParamValue)
        End Get
        Set(ByVal value As Integer)
            MyBase.Parameters(__AutoriseDev_parameter).ParamValue = value
        End Set
    End Property

    Public Overrides Sub PrepareComputation()

        'Calcul du nombre d'exécution par jour du modèle
        mNumberOfStepsPerDay = 86400.0F / CType(Application.Solver, SolverEuler1).dt

        mStadeLarvaire = mStadeLarvaireIni
        mAutoriseDev = mAutoriseDevIni
        mDaysWithoutWater = mDaysWithoutWaterIni

        'Row 0 : temperature associée
        'Row 1-4 : durée [jours] des stades L1 à L4
        Model = {{15.0F, 20.0F, 25.0F, 30.0F, 35.0F}, _
                {3.4F, 2.4F, 1.5F, 0.9F, 0.7F}, _
                {2.0F, 1.1F, 0.9F, 0.9F, 0.5F}, _
                {2.8F, 1.7F, 0.9F, 0.9F, 1.0F}, _
                {8.1F, 3.3F, 2.4F, 2.0F, 3.0F}}

    End Sub

    Public Overrides Sub ComputeOutput()

        'import du niveau de l'eau et de la température
        Dim _Level As Single = CSng(MyBase.Inputs(__Level_input).Value)
        Dim _T As Single = CSng(MyBase.Inputs(__TUp_input).Value)
        'declaration de la variable contenant les resultats de la fonction CalculateStadeLarvaire
        Dim mStadeLarvaireResult() As Single

        If _Level >= Me.z Then
            If mAutoriseDev = 1 Then
                'Lorsqu'une inondation a lieu et si le développement est autorisé (càd, si le terrain a été 
                'à sec pendant au moins 15 jours), on lance le calcul de développement larvaire
                mStadeLarvaireResult = Me.CalculateStadeLarvaire(mStadeLarvaire, _T, mAutoriseDev)
                mStadeLarvaire = mStadeLarvaireResult(0)
                mAutoriseDev = mStadeLarvaireResult(1)
            End If
            'puisqu'il y a inondation, le nombre de jours sans eau est remis à 0
            mDaysWithoutWater = 0
        Else
            'si le terrain est à sec, on incrémente la variable donnant le nombre de jours à sec
            mDaysWithoutWater += 1.0F / mNumberOfStepsPerDay
            If mDaysWithoutWater <= 3.0F Then
                If autoriseDev = 1 Then
                    'Si les larves sont privées d'eau, elles survivent quand meme quelques jours 
                    '(on choisit 5 jours, pour etre du coté de la sécurité...).
                    mStadeLarvaireResult = Me.CalculateStadeLarvaire(mStadeLarvaire, _T, mAutoriseDev)
                    mStadeLarvaire = mStadeLarvaireResult(0)
                    mAutoriseDev = mStadeLarvaireResult(1)
                End If
            ElseIf mDaysWithoutWater < 15.0F Then
                'Après ces 5 jours, les larves meurent
                mAutoriseDev = 0
                mStadeLarvaire = 0
            Else
                'lorsque le terrain a été pendant au moins 15 jours à sec, un nouveau développement
                'larvaire pourra avoir lieu lors de la prochaine inondation
                mAutoriseDev = 1
            End If
        End If

        MyBase.IntegralCount += 1
        MyBase.IntegralOutputs(__StadeLarvaire_result) = mStadeLarvaire * MyBase.IntegralCount
        MyBase.IntegralOutputs(__AutoriseDev_result) = mAutoriseDev * MyBase.IntegralCount
        MyBase.IntegralOutputs(__DaysWithoutWater_result) = mDaysWithoutWater * MyBase.IntegralCount

    End Sub

    Function CalculateStadeLarvaire(ByVal stadeLarvaire As Single, ByVal T As Single, ByVal autoriseDev As Single) As Single()

        'Declaration des variables de sortie
        Dim result As Single()
        ReDim result(2)
        Dim state_today As Integer = CInt(Math.Truncate(stadeLarvaire))
        Dim percentage_today As Single = stadeLarvaire - state_today

        Dim noCol As Integer = 4

        Dim i As Integer

        'percentage_incr montre l'augmentation du pourcentage de la maturation de l'état qui avait 
        'lieu entre hier et aujourd'hui
        Dim percentage_incr As Single

        'dans le cas où un changement d'état a lieu entre hier et aujourd'hui, time_new_state indique 
        'le temps qui s'est écoulé depuis que ce nouvel état a lieu.
        Dim time_new_state As Single

        If T < Model(0, 0) Or T > Model(0, noCol) Then
            If stadeLarvaire <> 0.0F Then
                'les larves meurent (si elles existent) lorsque la température se trouve en dehors des limites 
                'du modèle de développement larvaire.
                autoriseDev = 0
                stadeLarvaire = 0
            End If
        Else
            For i = 0 To noCol - 1
                If T >= Model(0, i) And T < Model(0, i + 1) Then
                    'percentage_incr est calculé de la manière suivante: on détermine par une interpolation 
                    'linéaire le nombre de jours nécessaires pour que le stade en cours soit complété. 
                    'Ensuite, on divise le pas de temps d'exécution du modèle par ce résultat pour obtenir 
                    'l'incrément de pourcentage d'accomplissement de ce stade lors de l'exécution en cours.
                    percentage_incr = CSng(1.0F / mNumberOfStepsPerDay / (Model(state_today, i) + (T - Model(0, i)) / (Model(0, i + 1) - Model(0, i)) * (Model(state_today, i + 1) - Model(state_today, i))))


                    If percentage_incr + percentage_today >= 1.0F Then
                        'lorsque le pourcentage d'accomplissement dépasse 1, cela signifie qu'un nouveau stade
                        'larvaire a été atteint.
                        If state_today = 4 Then
                            'Si le dernier stade vient d'etre complété, cela signifie que les larves ont atteint
                            'le stade adulte. Il faudra 15 jours d'assèchement pour qu'un nouveau développement
                            'débute.
                            time_new_state = 0
                            state_today = 0
                            percentage_today = 0
                            autoriseDev = 0
                        Else
                            'calcul du temps de développement dans le nouveau stade (en vue de calculer le 
                            'pourcentage de maturation du nouveau stade)
                            time_new_state = (percentage_incr + percentage_today - 1) / percentage_incr
                            'incrément de state_today pour atteindre le nouveau stade
                            state_today += 1
                            'calcul du pourcentage de la maturation du nouveau stade
                            percentage_today = time_new_state / mNumberOfStepsPerDay / (Model(state_today, i) + (T - Model(0, i)) / (Model(0, i + 1) - Model(0, i)) * (Model(state_today, i + 1) - Model(state_today, i)))
                        End If
                    Else
                        'lorsque le pourcentage d'accomplissement ne dépasse pas 1, cela signifie qu'on
                        'ne change pas de stade larvaire. Il y a uniquement une augmentation de la
                        'variable percentage_today
                        percentage_today += percentage_incr
                    End If
                End If
            Next
        End If
        result(0) = state_today + percentage_today
        result(1) = autoriseDev

        Return result

    End Function

    Public Overrides Sub SaveStep()
        For i As Integer = 0 To MyBase.Results.Count - 1
            MyBase.Results(i).Add(MyBase.IntegralOutputs(i) / MyBase.IntegralCount)
            MyBase.IntegralOutputs(i) = 0
        Next
        MyBase.IntegralCount = 0
    End Sub

    Public Overrides Sub SaveFirstStep()

        MyBase.Results(__StadeLarvaire_result).Add(mStadeLarvaireIni)
        MyBase.Results(__AutoriseDev_result).Add(mAutoriseDevIni)
        MyBase.Results(__DaysWithoutWater_result).Add(mDaysWithoutWaterIni)

        MyBase.IntegralOutputs(__StadeLarvaire_result) = 0
        MyBase.IntegralOutputs(__AutoriseDev_result) = 0
        MyBase.IntegralOutputs(__DaysWithoutWater_result) = 0
        MyBase.IntegralCount = 0
    End Sub

    Public Overrides ReadOnly Property GetInitialConditions() As System.Collections.Generic.List(Of Single)
        Get
            Dim L As New List(Of Single)
            L.Add(CSng(Me.Parameters(__stadelarvaire_parameter).ParamValue))
            L.Add(CSng(Me.Parameters(__AutoriseDev_parameter).ParamValue))
            L.Add(CSng(Me.Parameters(__DaysWithoutWater_parameter).ParamValue))
            Return L
        End Get
    End Property

    Public Overrides Sub DefaultStart(ByVal Coeff As Single)
        mStadeLarvaireIni = CSng(Me.Parameters(__stadelarvaire_parameter).ParamValue)
        mAutoriseDevIni = CInt(Me.Parameters(__AutoriseDev_parameter).ParamValue)
        mDaysWithoutWaterIni = CInt(Me.Parameters(__DaysWithoutWater_parameter).ParamValue)
    End Sub

    Public Overrides Sub SetStateParameters()
        Me.Parameters(__stadelarvaire_parameter).ParamValue = mStadeLarvaireIni
        Me.Parameters(__AutoriseDev_parameter).ParamValue = mAutoriseDevIni
        Me.Parameters(__DaysWithoutWater_parameter).ParamValue = mDaysWithoutWaterIni
    End Sub

    Public Overrides Sub Hotstart(ByVal index As Integer, ByVal Coeff As Single, Optional ByVal UpdateRange As Single = 1000)
        'Débit initial constant dans le tronçon
        If Results(__StadeLarvaire_result).X.Length >= index + 1 Then mStadeLarvaireIni = Results(__StadeLarvaire_result).X(index)
        If Results(__AutoriseDev_result).X.Length >= index + 1 Then mAutoriseDevIni = CInt(Results(__AutoriseDev_result).X(index))
        If Results(__DaysWithoutWater_result).X.Length >= index + 1 Then mDaysWithoutWaterIni = CInt(Results(__DaysWithoutWater_result).X(index))
    End Sub





    Public Overrides Sub Write(ByVal SW As System.IO.StreamWriter)
        SW.WriteLine("Mousticator")
        SW.WriteLine(1.001)
        MyBase.Write(SW)
    End Sub

    Public Overrides Sub Read(ByVal SR As System.IO.StreamReader)
        Dim V As Single
        V = CSng(SR.ReadLine)
        MyBase.Read(SR)
    End Sub
End Class
