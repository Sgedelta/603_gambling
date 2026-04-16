extends Button

func _on_pressed() -> void:
	get_tree().paused = false
	get_parent().get_parent().get_parent().queue_free()
	GameManager.GDInstance.PurchaseAdFree();
