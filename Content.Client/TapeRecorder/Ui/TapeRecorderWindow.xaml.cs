anager.InjectDependencies(this);

        _entMan = entMan;

        _owner = owner;

        _options = new RadioOptions<TapeRecorderMode>(RadioOptionsLayout.Horizontal);
        Buttons.AddChild(_options);
        _options.FirstButtonStyle = "OpenRight";
        _options.LastButtonStyle = "OpenLeft";
        _options.ButtonStyle = "OpenBoth";
        foreach (var mode in Enum.GetValues<TapeRecorderMode>())
        {
            var name = mode.ToString().ToLower();
            _options.AddItem(Loc.GetString($"tape-recorder-menu-{name}-button"), mode);
        }

        _options.OnItemSelected += args =>
        {
            if (_updating) // don't tell server to change mode to the mode it told us
                return;

            args.Button.Select(args.Id);
            var mode = args.Button.SelectedValue;
            OnModeChanged?.Invoke(mode);
        };

        PrintButton.OnPressed += _ => OnPrintTranscript?.Invoke();

        SetEnabled(TapeRecorderMode.Recording, false);
        SetEnabled(TapeRecorderMode.Playing, false);
        SetEnabled(TapeRecorderMode.Rewinding, false);
    }

    private void SetSlider(float maxTime, float currentTime)
    {
        PlaybackSlider.Disabled = true;
        PlaybackSlider.MaxValue = maxTime;
        PlaybackSlider.Value = currentTime;
    }

    public void UpdatePrint(bool disabled)
    {
        PrintButton.Disabled = disabled;
        _onCooldown = disabled;
    }

    public void UpdateState(TapeRecorderState state)
    {
        if (!_entMan.TryGetComponent<TapeRecorderComponent>(_owner, out var comp))
            return;

        _mode = comp.Mode; // TODO: update UI on handling state instead of adding UpdateUI to everything
        _hasCasette = state.HasCasette;

        _updating = true;

        CassetteLabel.Text = _hasCasette
            ? Loc.GetString("tape-recorder-menu-cassette-label", ("cassetteName", state.CassetteName))
            : Loc.GetString("tape-recorder-menu-no-cassette-label");

        // Select the currently used mode
        _options.SelectByValue(_mode);

        // When tape is ejected or a button can't be used, disable it
        // Server will change to paused once a tape is inactive
        var tapeLeft = state.CurrentTime < state.MaxTime;
        SetEnabled(TapeRecorderMode.Recording, tapeLeft);
        SetEnabled(TapeRecorderMode.Playing, tapeLeft);
        SetEnabled(TapeRecorderMode.Rewinding, state.CurrentTime > float.Epsilon);

        if (state.HasCasette)
            SetSlider(state.MaxTime, state.CurrentTime);

        _updating = false;
    }

    private void SetEnabled(TapeRecorderMode mode, bool condition)
    {
        _options.SetItemDisabled((int) mode, !(_hasCasette && condition));
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);

        if (!_entMan.HasComponent<ActiveTapeRecorderComponent>(_owner))
            return;

        if (!_entMan.TryGetComponent<TapeRecorderComponent>(_owner, out var comp))
            return;

        if (_mode != comp.Mode)
        {
            _mode = comp.Mode;
            _options.SelectByValue(_mode);
        }

        var speed = _mode == TapeRecorderMode.Rewinding
            ? -comp.RewindSpeed
            : 1f;
        PlaybackSlider.Value += args.DeltaSeconds * speed;
    }
}