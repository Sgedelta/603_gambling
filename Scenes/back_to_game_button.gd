extends Button

func _on_pressed() -> void:
	print("Close micro")
	get_parent().get_parent().ad_closed.emit()
	queue_free()
