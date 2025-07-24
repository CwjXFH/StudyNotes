# pip3 install transformers
# python3 deepseek_tokenizer.py
import transformers

chat_tokenizer_dir = "./"

tokenizer = transformers.AutoTokenizer.from_pretrained(
    chat_tokenizer_dir, trust_remote_code=True
)

token_ids = tokenizer.encode("大模型是未来的趋势")
for tid in token_ids:
    print(tid, tokenizer.decode([tid]))
