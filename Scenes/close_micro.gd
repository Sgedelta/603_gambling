extends Button

func _on_pressed() -> void:
	GameManager.GDInstance.ActiveMainGame.MicrotransactionOpen = false
	get_tree().paused = false
	owner.queue_free()
