# 困ったときに読む

## エラー内容がわからない

```
FailedToResolveParameterException: Failed to resolve parameter [SomeObject] for [SomeComponent]
```

このエラーは、```SomeComponent``` のインジェクションポイントにある、```SomeObject``` 引数が、DIコンテナに登録されていないことを示しています。
インストーラーに ```SomeObject``` のバインディングが記述されているか確認しましょう。
場合によっては、Args() によって引数を定義する必要があるかもしれません。

```
Exception: Already has binding for type [SomeObject]
```

このエラーは、```SomeObject``` 型が、二重にバインディングされようとしていることを意味しています。
インストーラーの記述を見直し、同じ型が二回バインドされていないかチェックしましょう。

> [!NOTE]
> Doinject は、一つの DIコンテナに対して、型の種類ごとに一つしかバインド記述を行うことができません。
> 複数のインスタンスが必要な場合は、ファクトリを作って生成するか、継承する・アダプタを置くなど、新しい型を用意することを検討してください。
> 型を作ることで、IDEサポートも受けやすくなりますし、より明確な意味を持ったコードとなるでしょう。
