extends Button

func _on_pressed() -> void:
	var payment = load("res://Scenes/AD_Microtransaction.tscn").instantiate()
	payment.process_mode = Node.PROCESS_MODE_ALWAYS
	payment.z_index = 100
	payment.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	get_tree().root.add_child(payment)
	get_parent().get_parent().get_parent().queue_free()
