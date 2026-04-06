extends Button

func _on_pressed() -> void:
	print("Close micro")
	get_tree().paused = false
	get_parent().get_parent().get_parent().queue_free()
