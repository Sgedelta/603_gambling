extends Button

func _on_pressed() -> void:
	get_tree().paused = false
	get_node("../../../../..").queue_free()
