extends Button

@onready var card_number   = $"../CardInformation/VBoxContainer2/CardNumber"
@onready var exp_date      = $"../CardInformation/VBoxContainer2/ExpDate"
@onready var security_code = $"../CardInformation/VBoxContainer2/SecurityCode"
@onready var card_name     = $"../CardInformation/VBoxContainer2/CardName"
@onready var first_name    = $"../BillingAddress/VBoxContainer/HBoxContainer/FirstName"
@onready var last_name     = $"../BillingAddress/VBoxContainer/HBoxContainer/LastName"
@onready var address_1     = $"../BillingAddress/VBoxContainer/Address1"
@onready var address_2     = $"../BillingAddress/VBoxContainer/Address2"
@onready var city          = $"../BillingAddress/VBoxContainer/City"
@onready var state         = $"../BillingAddress/VBoxContainer/State"
@onready var zip           = $"../BillingAddress/VBoxContainer/ZIP"
@onready var country       = $"../BillingAddress/VBoxContainer/Country"
@onready var phone_num     = $"../BillingAddress/VBoxContainer/PhoneNum"

var required_fields: Array
var optional_fields: Array
func _ready() -> void:
	required_fields = [card_number, exp_date, security_code, card_name,
		first_name, last_name, address_1, city, state, zip, country, phone_num]
	optional_fields = [address_2]

	for field in required_fields + optional_fields:
		field.text_changed.connect(func(_t): field.remove_theme_stylebox_override("normal"))
		
func _on_pressed() -> void:
	var empty_fields = required_fields.filter(func(f): return f.text.strip_edges() == "")

	if empty_fields.size() > 0:
		for field in empty_fields:
			field.add_theme_stylebox_override("normal", _error_style())
		return
	get_tree().root.find_child("ConfirmationLabel", true, false).visible = true
	
func _error_style() -> StyleBoxFlat:
	var s = StyleBoxFlat.new()
	s.bg_color = Color("#1A0A0A")
	s.border_color = Color("#FF4444")
	s.border_width_bottom = 2
	s.border_width_top    = 2
	s.border_width_left   = 2
	s.border_width_right  = 2
	s.corner_radius_top_left     = 8
	s.corner_radius_top_right    = 8
	s.corner_radius_bottom_left  = 8
	s.corner_radius_bottom_right = 8
	s.content_margin_left   = 12
	s.content_margin_right  = 12
	s.content_margin_top    = 10
	s.content_margin_bottom = 10
	return s
