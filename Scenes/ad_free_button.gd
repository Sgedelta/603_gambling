extends Button

func _on_pressed() -> void:
	var payment = load("res://Scenes/AD_Microtransaction.tscn").instantiate()
	payment.process_mode = Node.PROCESS_MODE_ALWAYS
	get_tree().root.add_child(payment)
